using AutoMapper;
using Salubrity.Application.DTOs.Email;
using Salubrity.Application.DTOs.HealthCamps;
using Salubrity.Application.Interfaces;
using Salubrity.Application.Interfaces.Repositories.HealthCamps;
using Salubrity.Application.Interfaces.Repositories.Lookups;
using Salubrity.Application.Interfaces.Repositories.Organizations;
using Salubrity.Application.Interfaces.Services.HealthCamps;
using Salubrity.Application.Interfaces.Services.HealthcareServices;
using Salubrity.Application.Interfaces.Storage;
using Salubrity.Domain.Entities.HealthCamps;
using Salubrity.Domain.Entities.HealthcareServices;
using Salubrity.Domain.Entities.Join;
using Salubrity.Domain.Entities.Lookup;
using Salubrity.Shared.Exceptions;

namespace Salubrity.Application.Services.HealthCamps;

public class HealthCampService : IHealthCampService
{
    private readonly IHealthCampRepository _repo;
    private readonly ILookupRepository<HealthCampStatus> _lookupRepository;
    private readonly IMapper _mapper;
    private readonly IPackageReferenceResolver _referenceResolver;
    private readonly ICampTokenFactory _tokenFactory;
    private readonly IFileStorage _files;

    private readonly IQrCodeService _qr;
    private readonly ITempPasswordService _tempPassword;
    private readonly IEmailService _email;
    private readonly IEmployeeReadRepository _employeeReadRepo;
    private static readonly string[] sourceArray = ["upcoming", "complete", "suspended"];

    public HealthCampService(IHealthCampRepository repo, ILookupRepository<HealthCampStatus> lookupRepository, IPackageReferenceResolver _pResolver, IMapper mapper, ICampTokenFactory tokenFactory, IEmailService emailService, IQrCodeService qrCodeService, ITempPasswordService tempPasswordService, IEmployeeReadRepository employeeReadRepo, IFileStorage files)
    {
        _repo = repo;
        _mapper = mapper;
        _referenceResolver = _pResolver;
        _lookupRepository = lookupRepository;
        _tokenFactory = tokenFactory ?? throw new ArgumentNullException(nameof(tokenFactory));
        _email = emailService;
        _qr = qrCodeService;
        _tempPassword = tempPasswordService;
        _employeeReadRepo = employeeReadRepo;
        _files = files ?? throw new ArgumentNullException(nameof(files));
    }

    public async Task<List<HealthCampListDto>> GetAllAsync()
    {
        var items = await _repo.GetAllAsync();
        return _mapper.Map<List<HealthCampListDto>>(items);
    }

    public async Task<HealthCampDetailDto> GetByIdAsync(Guid id)
    {
        var camp = await _repo.GetByIdAsync(id) ?? throw new NotFoundException("Camp not found");
        return _mapper.Map<HealthCampDetailDto>(camp);
    }


    public async Task<HealthCampDto> CreateAsync(CreateHealthCampDto dto)
    {
        var ct = CancellationToken.None; // or pass through
        var upcomingStatus = await _lookupRepository.FindByNameAsync("Upcoming");
        if (upcomingStatus == null || upcomingStatus.Id == Guid.Empty)
            throw new InvalidOperationException("Upcoming status not found");

        var entity = new HealthCamp
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            ServicePackageId = dto.ServicePackageId,
            Description = dto.Description,
            Location = dto.Location,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            StartTime = dto.StartTime,
            OrganizationId = dto.OrganizationId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            ExpectedParticipants = dto.ExpectedParticipants,
            HealthCampStatusId = upcomingStatus.Id,
            PackageItems = [],
            ServiceAssignments = [],
            Participants = [] // ensure initialized
        };

        // package items
        foreach (var item in dto.PackageItems)
        {
            var referenceType = await _referenceResolver.ResolveTypeAsync(item.ReferenceId);
            entity.PackageItems.Add(new HealthCampPackageItem
            {
                Id = Guid.NewGuid(),
                HealthCampId = entity.Id,
                ReferenceId = item.ReferenceId,
                ReferenceType = referenceType
            });
        }

        // service assignments
        foreach (var assignment in dto.ServiceAssignments)
        {
            entity.ServiceAssignments.Add(new HealthCampServiceAssignment
            {
                Id = Guid.NewGuid(),
                HealthCampId = entity.Id,
                ServiceId = assignment.ServiceId,
                SubcontractorId = assignment.SubcontractorId,
                ProfessionId = assignment.ProfessionId
            });
        }

        // NEW: seed participants = all active employees of the organization
        var employeeUserIds = await _employeeReadRepo.GetActiveEmployeeUserIdsAsync(dto.OrganizationId, ct);

        if (employeeUserIds.Count > 0)
        {
            // Avoid duplicates if any user id appears twice
            var uniqueUserIds = new HashSet<Guid>(employeeUserIds);

            foreach (var userId in uniqueUserIds)
            {
                entity.Participants.Add(new HealthCampParticipant
                {
                    Id = Guid.NewGuid(),
                    HealthCampId = entity.Id,
                    UserId = userId,
                    IsEmployee = true
                    // PatientId left null; can be resolved later if you link users->patients
                });
            }
        }

        var created = await _repo.CreateAsync(entity);
        return _mapper.Map<HealthCampDto>(created);
    }


    public async Task<HealthCampDto> UpdateAsync(Guid id, UpdateHealthCampDto dto)
    {
        var camp = await _repo.GetByIdAsync(id) ?? throw new NotFoundException("Camp not found");

        camp.Name = dto.Name;
        camp.Description = dto.Description;
        camp.Location = dto.Location;
        camp.StartDate = dto.StartDate;
        camp.EndDate = dto.EndDate;
        camp.IsActive = dto.IsActive;
        camp.UpdatedAt = DateTime.UtcNow;

        await _repo.UpdateAsync(camp);
        return _mapper.Map<HealthCampDto>(camp);
    }

    public async Task<LaunchHealthCampResponseDto> LaunchAsync(LaunchHealthCampDto dto)
    {
        var camp = await _repo.GetForLaunchAsync(dto.HealthCampId)
                   ?? throw new NotFoundException("Camp not found");

        // only 'Upcoming' camps can be launched
        if (camp.HealthCampStatus == null)
            throw new InvalidOperationException("Camp status is missing.");

        var upcomingStatus = await _lookupRepository
            .FindByNameAsync(camp.HealthCampStatus.Name)
            ?? throw new InvalidOperationException("'Upcoming' status not found");

        if (camp.HealthCampStatusId != upcomingStatus.Id)
            throw new ValidationException(["Only camps in 'Upcoming' status can be launched."]);


        var closeUtc = dto.CloseDate.ToUniversalTime();

        // Poster JTIs (for venue)
        camp.ParticipantPosterJti = Guid.NewGuid().ToString("N");
        camp.SubcontractorPosterJti = Guid.NewGuid().ToString("N");
        camp.PosterTokensExpireAt = closeUtc;

        // PARTICIPANTS (adjust property names to your model)
        foreach (var p in camp.Participants)
        {
            if (p.UserId == Guid.Empty || string.IsNullOrWhiteSpace(p.User.Email)) continue;

            var plain = _tempPassword.Generate(12);
            var hash = _tempPassword.Hash(plain);
            var jti = Guid.NewGuid().ToString("N");

            await _repo.UpsertTempCredentialAsync(new HealthCampTempCredentialUpsert
            {
                HealthCampId = camp.Id,
                UserId = p.UserId,
                Role = "participant",
                TempPasswordHash = hash,
                TempPasswordExpiresAt = closeUtc,
                SignInJti = jti,
                TokenExpiresAt = closeUtc
            });




            var token = _tokenFactory.CreateUserToken(camp.Id, p.UserId, "participant", jti, closeUtc);
            var url = _tokenFactory.BuildSignInUrl(token);

            var emailRequestDto = new EmailRequestDto
            {
                ToEmail = p.User.Email,
                Subject = "Health Camp Invitation: " + camp.Name,
                TemplateKey = "HealthCampInvitation",
                Model = new
                {
                    FullName = p.User.FullName ?? "Participant",
                    SignInUrl = url,
                    TempPassword = plain,
                    ExpiryDate = closeUtc
                }
            };

            await _email.SendAsync(emailRequestDto);
        }

        // SUBCONTRACTORS (adjust property names to your model)
        foreach (var a in camp.ServiceAssignments)
        {
            if (a.SubcontractorId == Guid.Empty || string.IsNullOrWhiteSpace(a.Subcontractor.User.Email)) continue;

            var plain = _tempPassword.Generate(12);
            var hash = _tempPassword.Hash(plain);
            var jti = Guid.NewGuid().ToString("N");

            await _repo.UpsertTempCredentialAsync(new HealthCampTempCredentialUpsert
            {
                HealthCampId = camp.Id,
                UserId = a.SubcontractorId,
                Role = "subcontractor",
                TempPasswordHash = hash,
                TempPasswordExpiresAt = closeUtc,
                SignInJti = jti,
                TokenExpiresAt = closeUtc
            });

            var token = _tokenFactory.CreateUserToken(camp.Id, a.SubcontractorId, "subcontractor", jti, closeUtc);
            var url = _tokenFactory.BuildSignInUrl(token);

            var emailRequestDto = new EmailRequestDto
            {
                ToEmail = a.Subcontractor.User.Email,
                Subject = "Health Camp Invitation: " + camp.Name,
                TemplateKey = "HealthCampInvitation",
                Model = new
                {
                    FullName = a.Subcontractor.User.FullName ?? "Participant",
                    SignInUrl = url,
                    TempPassword = plain,
                    ExpiryDate = closeUtc
                }
            };
            await _email.SendAsync(emailRequestDto);
        }


        var ongoingStatus = await _lookupRepository.FindByNameAsync("Ongoing") ?? throw new InvalidOperationException("Ongoing status not found");
        camp.HealthCampStatusId = ongoingStatus.Id;



        // Persist changes
        await _repo.UpdateAsync(camp);
        await _repo.SaveChangesAsync();

        // Build poster QR codes 
        var participantPosterToken = _tokenFactory.CreatePosterToken(camp.Id, "participant", camp.ParticipantPosterJti!, closeUtc);
        var subcontractorPosterToken = _tokenFactory.CreatePosterToken(camp.Id, "subcontractor", camp.SubcontractorPosterJti!, closeUtc);

        var participantPosterUrl = _tokenFactory.BuildSignInUrl(participantPosterToken);
        var subcontractorPosterUrl = _tokenFactory.BuildSignInUrl(subcontractorPosterToken);

        var patientQrBase64 = _qr.GenerateBase64Png(participantPosterUrl);
        var subcoQrBase64 = _qr.GenerateBase64Png(subcontractorPosterUrl);

        //  write PNGs to disk under wwwroot/qrcodes/healthcamps/{campId}/
        var folder = $"qrcodes/healthcamps/{camp.Id:N}";
        var participantBytes = DecodeBase64Png(patientQrBase64);
        var subcontractorBytes = DecodeBase64Png(subcoQrBase64);

        // Filenames can carry JTI and expiry to avoid stale caching
        var participantFile = $"participant_{camp.ParticipantPosterJti}_{closeUtc:yyyyMMddHHmmss}.png";
        var subcontractorFile = $"subcontractor_{camp.SubcontractorPosterJti}_{closeUtc:yyyyMMddHHmmss}.png";

        var participantPngUrl = await _files.SaveAsync(participantBytes, folder, participantFile, "image/png");
        var subcontractorPngUrl = await _files.SaveAsync(subcontractorBytes, folder, subcontractorFile, "image/png");

        // Return URLs 
        return new LaunchHealthCampResponseDto
        {
            HealthCampId = camp.Id,
            CloseDate = closeUtc,
            ParticipantPosterQrUrl = participantPngUrl,
            SubcontractorPosterQrUrl = subcontractorPngUrl
        };
    }


    private static byte[] DecodeBase64Png(string base64)
    {
        // handle data URI prefix if present
        const string prefix = "data:image/png;base64,";
        if (base64.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            base64 = base64[prefix.Length..];

        return Convert.FromBase64String(base64);
    }


    public async Task DeleteAsync(Guid id)
    {
        var camp = await _repo.GetByIdAsync(id) ?? throw new NotFoundException("Camp not found");
        await _repo.DeleteAsync(camp.Id);
    }
    //  Use nullable Guid
    public async Task<List<HealthCampListDto>> GetMyUpcomingCampsAsync(Guid? subcontractorId)
    {
        var camps = subcontractorId is null
            ? await _repo.GetAllUpcomingCampsAsync()
            : await _repo.GetMyUpcomingCampsAsync(subcontractorId.Value);

        return _mapper.Map<List<HealthCampListDto>>(camps);
    }

    public async Task<List<HealthCampListDto>> GetMyCompleteCampsAsync(Guid? subcontractorId)
    {
        var camps = subcontractorId is null
            ? await _repo.GetAllCompleteCampsAsync()
            : await _repo.GetMyCompleteCampsAsync(subcontractorId.Value);

        return _mapper.Map<List<HealthCampListDto>>(camps);
    }

    public async Task<List<HealthCampListDto>> GetMyCanceledCampsAsync(Guid? subcontractorId)
    {
        var camps = subcontractorId is null
            ? await _repo.GetAllCanceledCampsAsync()
            : await _repo.GetMyCanceledCampsAsync(subcontractorId.Value);

        return _mapper.Map<List<HealthCampListDto>>(camps);
    }

    // These stay the same
    public Task<List<CampParticipantListDto>> GetCampParticipantsAllAsync(Guid campId, string? q, string? sort, int page, int pageSize)
        => _repo.GetCampParticipantsAllAsync(campId, q, sort, page, pageSize);

    public Task<List<CampParticipantListDto>> GetCampParticipantsServedAsync(Guid campId, string? q, string? sort, int page, int pageSize)
        => _repo.GetCampParticipantsServedAsync(campId, q, sort, page, pageSize);

    public Task<List<CampParticipantListDto>> GetCampParticipantsNotSeenAsync(Guid campId, string? q, string? sort, int page, int pageSize)
        => _repo.GetCampParticipantsNotSeenAsync(campId, q, sort, page, pageSize);

    // Status-based camps with optional subcontractor
    public async Task<List<HealthCampWithRolesDto>> GetMyCampsWithRolesByStatusAsync(Guid? subcontractorId, string status, CancellationToken ct = default)
    {
        if (!sourceArray.Contains(status))
            throw new ValidationException(["Invalid camp status filter."]);

        var camps = await _repo.GetMyCampsWithRolesByStatusAsync(subcontractorId ?? Guid.Empty, status, ct);
        return _mapper.Map<List<HealthCampWithRolesDto>>(camps);
    }

    public Task<List<HealthCampPatientDto>> GetCampPatientsByStatusAsync(
        Guid campId,
        string filter,
        string? q,
        string? sort,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        return _repo.GetCampPatientsByStatusAsync(campId, filter, q, sort, page, pageSize, ct);
    }

    //  Already uses nullable subcontractorId
    public async Task<CampPatientDetailWithFormsDto> GetCampPatientDetailWithFormsForCurrentAsync(
        Guid campId,
        Guid participantId,
        Guid? subcontractorIdOrNullForAdmin,
        CancellationToken ct = default)
    {
        var dto = await _repo.GetCampPatientDetailWithFormsAsync(
            campId, participantId, subcontractorIdOrNullForAdmin, ct);

        if (dto == null)
            throw new NotFoundException("Participant not found in this camp.");

        return dto;
    }

}