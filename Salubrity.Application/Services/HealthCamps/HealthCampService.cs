using AutoMapper;
using Microsoft.IdentityModel.Tokens;
using Salubrity.Application.Common.Interfaces.Repositories;
using Salubrity.Application.DTOs.Email;
using Salubrity.Application.DTOs.HealthCamps;
using Salubrity.Application.DTOs.Rbac;
using Salubrity.Application.Interfaces;
using Salubrity.Application.Interfaces.Repositories;
using Salubrity.Application.Interfaces.Repositories.HealthCamps;
using Salubrity.Application.Interfaces.Repositories.Lookups;
using Salubrity.Application.Interfaces.Repositories.Organizations;
using Salubrity.Application.Interfaces.Repositories.Rbac;
using Salubrity.Application.Interfaces.Security;
using Salubrity.Application.Interfaces.Services.HealthCamps;
using Salubrity.Application.Interfaces.Services.HealthcareServices;
using Salubrity.Application.Interfaces.Services.Notifications;
using Salubrity.Application.Interfaces.Storage;
using Salubrity.Domain.Entities.HealthCamps;
using Salubrity.Domain.Entities.HealthcareServices;
using Salubrity.Domain.Entities.Join;
using Salubrity.Domain.Entities.Lookup;
using Salubrity.Domain.Entities.Subcontractor;
using Salubrity.Shared.Exceptions;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Salubrity.Application.Services.HealthCamps;

public class HealthCampService : IHealthCampService
{
    private readonly IHealthCampRepository _repo;
    private readonly ILookupRepository<HealthCampStatus> _lookupRepository;

    private readonly ILookupRepository<SubcontractorHealthCampAssignmentStatus> _lookupSubcontractorHealthCampAssignmentRepository;
    private readonly IMapper _mapper;
    private readonly IPackageReferenceResolver _referenceResolver;
    private readonly ICampTokenFactory _tokenFactory;
    private readonly IJwtService _jwt;
    private readonly IFileStorage _files;

    private readonly IQrCodeService _qr;
    private readonly ITempPasswordService _tempPassword;
    private readonly IEmailService _email;
    private readonly IEmployeeReadRepository _employeeReadRepo;
    private readonly ISubcontractorCampAssignmentRepository _subcontractorCampAssignmentRepository;
    private static readonly string[] sourceArray = ["upcoming", "complete", "suspended"];
    private readonly INotificationService _notificationService;
    private readonly IHealthCampParticipantRepository _campParticipantRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IHealthCampParticipantPackageRepository _participantPackageRepo;
    private readonly IHealthCampPackageRepository _campPackageRepository;

    public HealthCampService(IHealthCampPackageRepository campPackageRepository, IHealthCampRepository repo, ILookupRepository<HealthCampStatus> lookupRepository, IPackageReferenceResolver _pResolver, IMapper mapper, ICampTokenFactory tokenFactory, IEmailService emailService, IQrCodeService qrCodeService, ITempPasswordService tempPasswordService, IEmployeeReadRepository employeeReadRepo, IFileStorage files, ISubcontractorCampAssignmentRepository subcontractorCampAssignment, ILookupRepository<SubcontractorHealthCampAssignmentStatus> lookupSubcontractorHealthCampAssignmentRepository, INotificationService notificationService, IHealthCampParticipantRepository campParticipantRepository, IJwtService jwt, IRoleRepository roleRepository, IHealthCampParticipantPackageRepository participantPackageRepo)
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
        _subcontractorCampAssignmentRepository = subcontractorCampAssignment ?? throw new ArgumentNullException(nameof(subcontractorCampAssignment));
        _lookupSubcontractorHealthCampAssignmentRepository = lookupSubcontractorHealthCampAssignmentRepository ?? throw new ArgumentNullException(nameof(lookupSubcontractorHealthCampAssignmentRepository));
        _notificationService = notificationService;
        _campParticipantRepository = campParticipantRepository ?? throw new ArgumentNullException(nameof(campParticipantRepository));
        _jwt = jwt ?? throw new ArgumentNullException(nameof(jwt));
        _roleRepository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));
        _participantPackageRepo = participantPackageRepo ?? throw new ArgumentNullException(nameof(participantPackageRepo));
        _campPackageRepository = campPackageRepository;
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
        var ct = CancellationToken.None;

        var upcomingStatus = await _lookupRepository.FindByNameAsync("Upcoming");
        if (upcomingStatus == null || upcomingStatus.Id == Guid.Empty)
            throw new InvalidOperationException("Upcoming status not found");

        // ───────────────────────────────────────────────
        // 🔧 Helper for UTC-safe conversion
        // ───────────────────────────────────────────────
        static DateTime ToUtc(DateTime value)
        {
            // Avoid breaking when already UTC or local
            if (value.Kind == DateTimeKind.Utc)
                return value;
            if (value.Kind == DateTimeKind.Local)
                return value.ToUniversalTime();
            // Explicitly mark as UTC if unspecified
            return DateTime.SpecifyKind(value, DateTimeKind.Utc);
        }

        static DateTime? ToUtcNullable(DateTime? value)
        {
            if (!value.HasValue) return null;
            return ToUtc(value.Value);
        }

        // ───────────────────────────────────────────────
        // Initialize base camp entity
        // ───────────────────────────────────────────────
        var entity = new HealthCamp
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Description = dto.Description,
            Location = dto.Location,

            // Force proper UTC for PostgreSQL
            StartDate = ToUtc(dto.StartDate.Date.AddDays(1)),
            EndDate = dto.EndDate.HasValue ? ToUtc(dto.EndDate.Value.Date.AddDays(1)) : null,

            StartTime = dto.StartTime,
            OrganizationId = dto.OrganizationId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            ExpectedParticipants = dto.ExpectedParticipants,
            HealthCampStatusId = upcomingStatus.Id,
            PackageItems = [],
            ServiceAssignments = [],
            Participants = []
        };

        // ───────────────────────────────────────────────
        // Loop through packages
        // ───────────────────────────────────────────────
        if (dto.Packages != null && dto.Packages.Any())
        {
            foreach (var package in dto.Packages)
            {
                // Add package linkage
                entity.HealthCampPackages.Add(new HealthCampPackage
                {
                    Id = Guid.NewGuid(),
                    HealthCampId = entity.Id,
                    ServicePackageId = package.PackageId,
                    IsActive = true
                });

                // Add package items
                if (package.PackageItems != null)
                {
                    foreach (var item in package.PackageItems)
                    {
                        var referenceType = await _referenceResolver.ResolveTypeAsync(item.ReferenceId);
                        entity.PackageItems.Add(new HealthCampPackageItem
                        {
                            Id = Guid.NewGuid(),
                            HealthCampId = entity.Id,
                            ReferenceId = item.ReferenceId,
                            ServicePackageId = package.PackageId,
                            ReferenceType = referenceType,
                        });
                    }
                }

                // Add service assignments
                if (package.ServiceAssignments != null)
                {
                    foreach (var assignment in package.ServiceAssignments)
                    {
                        var referenceType = await _referenceResolver.ResolveTypeAsync(assignment.ServiceId);

                        entity.ServiceAssignments.Add(new HealthCampServiceAssignment
                        {
                            Id = Guid.NewGuid(),
                            HealthCampId = entity.Id,
                            AssignmentId = assignment.ServiceId,
                            AssignmentType = (PackageItemType)referenceType,
                            SubcontractorId = assignment.SubcontractorId,
                            ProfessionId = assignment.ProfessionId
                        });
                    }
                }
            }
        }

        // ───────────────────────────────────────────────
        // Add default participants (employees)
        // ───────────────────────────────────────────────
        var employeeUserIds = await _employeeReadRepo.GetActiveEmployeeUserIdsAsync(dto.OrganizationId, ct);
        if (employeeUserIds.Count > 0)
        {
            foreach (var userId in employeeUserIds.Distinct())
            {
                entity.Participants.Add(new HealthCampParticipant
                {
                    Id = Guid.NewGuid(),
                    HealthCampId = entity.Id,
                    UserId = userId,
                    IsEmployee = true
                });
            }
        }

        // ───────────────────────────────────────────────
        // Persist camp entity
        // ───────────────────────────────────────────────
        var created = await _repo.CreateAsync(entity);

        await _notificationService.TriggerNotificationAsync(
            title: "New Health Camp Created",
            message: $"A new health camp '{created.Name}' has been created.",
            type: "HealthCamp",
            entityId: created.Id,
            entityType: "Camp",
            ct: ct
        );

        // ───────────────────────────────────────────────
        // Create subcontractor booth assignments
        // ───────────────────────────────────────────────
        var assignedStatus = await _lookupSubcontractorHealthCampAssignmentRepository.FindByNameAsync("Pending");
        if (assignedStatus == null)
            throw new InvalidOperationException("Assignment status 'Pending' not found");

        foreach (var assignment in entity.ServiceAssignments)
        {
            var boothLabel = $"Booth-{Guid.NewGuid().ToString()[..4].ToUpper()}";

            var boothAssignment = new SubcontractorHealthCampAssignment
            {
                Id = Guid.NewGuid(),
                HealthCampId = created.Id,
                SubcontractorId = assignment.SubcontractorId,
                AssignmentStatusId = assignedStatus.Id,
                BoothLabel = boothLabel,

                // ✅ Always store UTC values for PostgreSQL
                StartDate = ToUtc(created.StartDate),
                CreatedAt = DateTime.UtcNow,

                IsDeleted = false,
                IsPrimaryAssignment = true,
                AssignmentId = assignment.AssignmentId,
                AssignmentType = assignment.AssignmentType
            };

            await _subcontractorCampAssignmentRepository.AddAsync(boothAssignment);
        }

        return _mapper.Map<HealthCampDto>(created);
    }




    public async Task<HealthCampDto> UpdateAsync(Guid id, UpdateHealthCampDto dto)
    {
        var ct = CancellationToken.None;
        var camp = await _repo.GetByIdWithPackagesAsync(id)
            ?? throw new NotFoundException("Camp not found");

        // 🔧 Basic field updates
        if (!string.IsNullOrWhiteSpace(dto.Name)) camp.Name = dto.Name;
        if (!string.IsNullOrWhiteSpace(dto.Description)) camp.Description = dto.Description;
        if (!string.IsNullOrWhiteSpace(dto.Location)) camp.Location = dto.Location;
        if (dto.StartDate.HasValue) camp.StartDate = dto.StartDate.Value;
        if (dto.EndDate.HasValue) camp.EndDate = dto.EndDate.Value;
        if (dto.StartTime.HasValue) camp.StartTime = dto.StartTime.Value;
        if (dto.IsActive.HasValue) camp.IsActive = dto.IsActive.Value;
        if (dto.ExpectedParticipants.HasValue) camp.ExpectedParticipants = dto.ExpectedParticipants.Value;
        if (dto.OrganizationId.HasValue) camp.OrganizationId = dto.OrganizationId.Value;

        camp.UpdatedAt = DateTime.UtcNow;

        // 🧩 Handle updated packages if provided
        if (dto.Packages is not null && dto.Packages.Any())
        {
            // Remove inactive packages
            foreach (var pkg in camp.HealthCampPackages)
                pkg.IsActive = false;

            // Add or reactivate provided packages
            foreach (var dtoPkg in dto.Packages)
            {
                var existingPkg = camp.HealthCampPackages
                    .FirstOrDefault(p => p.ServicePackageId == dtoPkg.PackageId);

                if (existingPkg != null)
                {
                    existingPkg.IsActive = true;
                    existingPkg.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    camp.HealthCampPackages.Add(new HealthCampPackage
                    {
                        Id = Guid.NewGuid(),
                        HealthCampId = camp.Id,
                        ServicePackageId = dtoPkg.PackageId,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }
        }

        await _repo.UpdateAsync(camp);

        await _notificationService.TriggerNotificationAsync(
            title: "Health Camp Updated",
            message: $"Health camp '{camp.Name}' has been updated.",
            type: "HealthCamp",
            entityId: camp.Id,
            entityType: "Camp",
            ct: ct
        );

        return _mapper.Map<HealthCampDto>(camp);
    }


    public async Task<LaunchHealthCampResponseDto> LaunchAsync(LaunchHealthCampDto dto)
    {
        var ct = CancellationToken.None;
        var camp = await _repo.GetForLaunchAsync(dto.HealthCampId)
                   ?? throw new NotFoundException("Camp not found");

        if (camp.HealthCampStatus == null)
            throw new InvalidOperationException("Camp status is missing.");

        var upcomingStatus = await _lookupRepository
            .FindByNameAsync(camp.HealthCampStatus.Name)
            ?? throw new InvalidOperationException("'Upcoming' status not found");

        if (camp.HealthCampStatusId != upcomingStatus.Id)
            throw new ValidationException(["Only camps in 'Upcoming' status can be launched."]);

        var eat = TimeZoneInfo.FindSystemTimeZoneById("Africa/Nairobi");
        var nowUtc = DateTime.UtcNow;
        var todayLocal = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, eat).Date;

        var startDate = camp.StartDate.Date;
        var endDate = (camp.EndDate ?? camp.StartDate).Date;

        if (todayLocal < startDate)
            throw new ValidationException([$"You can only launch this camp on or after its start date: {startDate:dd MMM yyyy}."]);

        if (todayLocal > endDate)
            throw new ValidationException([$"This camp already ended on {endDate:dd MMM yyyy} and cannot be launched."]);

        var closeUtc = dto.CloseDate.ToUniversalTime();

        await _notificationService.TriggerNotificationAsync(
            title: "Health Camp Launched",
            message: $"Health camp '{camp.Name}' has been launched.",
            type: "HealthCamp",
            entityId: camp.Id,
            entityType: "Camp",
            ct: ct
        );

        // Assign new JTI and expiry
        camp.ParticipantPosterJti = Guid.NewGuid().ToString("N");
        camp.SubcontractorPosterJti = Guid.NewGuid().ToString("N");
        camp.PosterTokensExpireAt = closeUtc;

        var participantRole = await _roleRepository.FindByNameAsync("participant") ?? await _roleRepository.FindByNameAsync("patient");
        var subcontractorRole = await _roleRepository.FindByNameAsync("subcontractor");
        if (participantRole == null || subcontractorRole == null)
            throw new InvalidOperationException("Role not found.");

        // Generate QR codes early
        var participantPosterToken = _tokenFactory.CreatePosterToken(camp.Id, "participant", participantRole.Id, camp.ParticipantPosterJti!, closeUtc, camp.OrganizationId);
        var subcontractorPosterToken = _tokenFactory.CreatePosterToken(camp.Id, "subcontractor", subcontractorRole.Id, camp.SubcontractorPosterJti!, closeUtc);

        var participantPosterUrl = _tokenFactory.BuildSignInUrl(participantPosterToken);
        var subcontractorPosterUrl = _tokenFactory.BuildSignInUrl(subcontractorPosterToken);

        var patientQrBase64 = _qr.GenerateBase64Png(participantPosterUrl);
        var subcoQrBase64 = _qr.GenerateBase64Png(subcontractorPosterUrl);

        // Send participant emails
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
                    ExpiryDate = closeUtc,
                    QrCodeBase64 = patientQrBase64
                }
            };



            await _email.SendAsync(emailRequestDto);



        }

        // Send subcontractor emails
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
                    FullName = a.Subcontractor.User.FullName ?? "Subcontractor",
                    SignInUrl = url,
                    TempPassword = plain,
                    ExpiryDate = closeUtc,
                    QrCodeBase64 = subcoQrBase64
                }
            };

            await _email.SendAsync(emailRequestDto);
        }

        // Finalize status
        var ongoingStatus = await _lookupRepository.FindByNameAsync("Ongoing")
                             ?? throw new InvalidOperationException("Ongoing status not found");

        camp.HealthCampStatusId = ongoingStatus.Id;
        camp.IsLaunched = true;
        camp.CloseDate = closeUtc;


        await _repo.UpdateAsync(camp);


        // Save QR PNGs for dashboard posters
        var folder = $"qrcodes/healthcamps/{camp.Id:N}";
        var participantBytes = DecodeBase64Png(patientQrBase64);
        var subcontractorBytes = DecodeBase64Png(subcoQrBase64);

        var participantFile = $"participant_{camp.ParticipantPosterJti}_{closeUtc:yyyyMMddHHmmss}.png";
        var subcontractorFile = $"subcontractor_{camp.SubcontractorPosterJti}_{closeUtc:yyyyMMddHHmmss}.png";

        var participantPngUrl = await _files.SaveAsync(participantBytes, folder, participantFile, "image/png");
        var subcontractorPngUrl = await _files.SaveAsync(subcontractorBytes, folder, subcontractorFile, "image/png");

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
        const string prefix = "data:image/png;base64,";
        if (base64.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            base64 = base64[prefix.Length..];

        return Convert.FromBase64String(base64);
    }


    public async Task DeleteAsync(Guid id, Guid userId)
    {
        var camp = await _repo.GetByIdAsync(id) ?? throw new NotFoundException("Camp not found");

        // Guardrails
        var tz = TimeZoneInfo.FindSystemTimeZoneById("Africa/Nairobi");
        var todayLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz).Date;

        var start = camp.StartDate.Date;
        var end = (camp.EndDate ?? camp.StartDate).Date;

        // If ongoing or completed -> do NOT delete; advise cancel/archive
        var hasStarted = todayLocal >= start;
        var hasEnded = todayLocal > end;
        var isLaunched = camp.IsLaunched;

        if (isLaunched || hasStarted || hasEnded)
            throw new ValidationException([
                "You cannot delete a launched or past camp. Use 'Cancel' (pre-start) or keep it for records."
            ]);

        // (Optional) safety checks: no assessments, no irreversible data, etc.
        // if (await _repo.HasAnyAssessmentsAsync(camp.Id)) throw new ValidationException([...]);

        // Soft delete
        camp.IsDeleted = true;
        camp.DeletedAt = DateTime.UtcNow;
        camp.DeletedBy = userId;
        await _repo.UpdateAsync(camp); // or _repo.SoftDeleteAsync(camp)

        // (Optional) cascade soft-delete light-touch for related joins created at camp creation time
        // await _repo.SoftDeleteParticipantsForCampAsync(camp.Id);
        // await _repo.SoftDeleteAssignmentsForCampAsync(camp.Id);
    }

    //  Use nullable Guid
    public async Task<List<HealthCampListDto>> GetMyUpcomingCampsAsync(Guid? subcontractorId)
    {
        var camps = subcontractorId is null
            ? await _repo.GetAllUpcomingCampsAsync()
            : await _repo.GetMyUpcomingCampsAsync(subcontractorId.Value);

        return _mapper.Map<List<HealthCampListDto>>(camps);
    }

    public async Task<List<HealthCampListDto>> GetMyOngoingCampsAsync(Guid? subcontractorId)
    {
        var camps = subcontractorId is null
            ? await _repo.GetAllOngoingCampsAsync()
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
    public Task<List<CampParticipantListDto>> GetCampParticipantsAllAsync(Guid campId, Guid? serviceAssignmentId, string? q, string? sort, int page, int pageSize, CancellationToken ct = default)

     => _repo.GetCampParticipantsAllAsync(campId, serviceAssignmentId, q, sort, page, pageSize);

    public Task<List<CampParticipantListDto>> GetCampParticipantsServedAsync(Guid campId, Guid? serviceAssignmentId, string? q, string? sort, int page, int pageSize, CancellationToken ct = default)

     => _repo.GetCampParticipantsServedAsync(campId, serviceAssignmentId, q, sort, page, pageSize);

    public Task<List<CampParticipantListDto>> GetCampParticipantsNotSeenAsync(Guid campId, Guid? serviceAssignmentId, string? q, string? sort, int page, int pageSize, CancellationToken ct = default)

        => _repo.GetCampParticipantsNotSeenAsync(campId, serviceAssignmentId, q, sort, page, pageSize);

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

    public async Task<List<OrganizationCampListDto>> GetCampsByOrganizationAsync(Guid organizationId, CancellationToken ct = default)
    {
        return await _repo.GetCampsByOrganizationAsync(organizationId, ct);
    }

    public async Task<OrganizationStatsDto> GetOrganizationStatsAsync(Guid organizationId, CancellationToken ct = default)
    {
        return await _repo.GetOrganizationStatsAsync(organizationId, ct);
    }

    public async Task<List<DateTime>> GetUpcomingCampDatesAsync(CancellationToken ct = default)
    {
        return await _repo.GetUpcomingCampDatesAsync(ct);
    }

    public async Task<CampLinkResultDto> TryLinkUserToCampAsync(Guid userId, string campToken, CancellationToken ct = default)
    {
        var result = new CampLinkResultDto();

        try
        {
            var principal = _jwt.ValidateToken(campToken, "camp-signin", "salubrity-api");

            var campIdStr = principal.FindFirst("campId")?.Value;
            if (!Guid.TryParse(campIdStr, out var campId))
            {
                result.Warnings.Add("Camp token is invalid.");
                return result;
            }

            result.CampId = campId;

            var camp = await _repo.GetByIdAsync(campId);
            if (camp is null)
            {
                result.Warnings.Add("Camp not found.");
                return result;
            }

            // Don't link to past camps
            var today = DateTime.UtcNow.Date;
            if (camp.EndDate.HasValue && camp.EndDate.Value.Date < today)
            {
                result.Warnings.Add("Camp has already ended.");
                return result;
            }

            var alreadyLinked = await _campParticipantRepository.IsParticipantLinkedToCampAsync(campId, userId, ct);
            if (alreadyLinked)
            {
                result.Linked = true;
                result.Info.Add("User already linked to camp.");
                return result;
            }

            var participant = new HealthCampParticipant
            {
                Id = Guid.NewGuid(),
                HealthCampId = campId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            await _campParticipantRepository.AddParticipantAsync(participant, ct);

            result.Linked = true;
            result.Info.Add("User linked to camp.");
        }
        catch (SecurityTokenExpiredException)
        {
            result.Warnings.Add("Camp token expired.");
        }
        catch (Exception)
        {
            result.Warnings.Add("Unexpected error during camp linking.");
        }

        return result;
    }

    public async Task<CampLinkResultDto> LinkUserToCampByIdAsync(Guid userId, Guid campId, CancellationToken ct = default)
    {
        var result = new CampLinkResultDto
        {
            CampId = campId
        };

        var camp = await _repo.GetByIdAsync(campId);
        if (camp is null)
        {
            result.Warnings.Add("Camp not found.");
            return result;
        }

        var today = DateTime.UtcNow.Date;
        if (camp.EndDate.HasValue && camp.EndDate.Value.Date < today)
        {
            result.Warnings.Add("Camp has already ended.");
            return result;
        }

        var alreadyLinked = await _campParticipantRepository.IsParticipantLinkedToCampAsync(campId, userId, ct);
        if (alreadyLinked)
        {
            result.Linked = true;
            result.Info.Add("User already linked to camp.");
            return result;
        }

        var participant = new HealthCampParticipant
        {
            Id = Guid.NewGuid(),
            HealthCampId = campId,
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        await _campParticipantRepository.AddParticipantAsync(participant, ct);

        result.Linked = true;
        result.Info.Add("User successfully linked to camp.");
        return result;
    }

    public async Task UpdateParticipantBillingStatusAsync(Guid campId, Guid participantId, UpdateParticipantBillingStatusDto dto, CancellationToken ct = default)
    {
        var participant = await _campParticipantRepository.GetParticipantAsync(campId, participantId, ct);
        if (participant is null)
        {
            throw new NotFoundException("Participant not found in this camp.");
        }

        participant.BillingStatusId = dto.BillingStatusId;
        await _campParticipantRepository.UpdateParticipantAsync(participant, ct);
    }

    public async Task<ParticipantBillingStatusDto> GetParticipantBillingStatusAsync(Guid campId, Guid participantId, CancellationToken ct = default)
    {
        var participant = await _campParticipantRepository.GetParticipantWithBillingStatusAsync(campId, participantId, ct);
        if (participant is null || participant.BillingStatus is null)
        {
            throw new NotFoundException("Participant or billing status not found.");
        }

        return new ParticipantBillingStatusDto
        {
            Id = participant.BillingStatus.Id,
            Name = participant.BillingStatus.Name
        };
    }

    public async Task<QrEncodingDetailDto> DecodePosterTokenAsync(string token, CancellationToken ct)
    {
        var principal = _jwt.ValidateToken(token, "camp-signin", "salubrity-api");
        if (principal == null)
            throw new ValidationException(["Invalid or expired token."]);

        var claims = principal.Claims.ToList();

        var campId = Guid.Parse(claims.First(c => c.Type == "campId").Value);
        var role = claims.First(c => c.Type == ClaimTypes.Role).Value;
        var userId = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        var jti = claims.First(c => c.Type == "jti").Value;

        // parse exp as unix time
        var expClaim = claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Exp)?.Value;
        var expiresAt = expClaim != null
            ? DateTimeOffset.FromUnixTimeSeconds(long.Parse(expClaim))
            : DateTimeOffset.MinValue;

        return new QrEncodingDetailDto
        {
            Token = token,
            CampId = campId,
            Role = role,
            UserId = userId,
            IsPoster = claims.Any(c => c.Type == "poster"),
            ExpiresAt = expiresAt,
            Jti = jti
        };
    }

    public async Task AddSubcontractorToCampAsync(Guid campId, ModifySubcontractorCampDto dto, Guid actingUserId)
    {
        var ct = CancellationToken.None;

        var camp = await _repo.GetByIdAsync(campId);
        if (camp == null)
            throw new NotFoundException("Health Camp", campId.ToString());

        // Verify subcontractor exists
        var subcontractorAssignments = await _subcontractorCampAssignmentRepository
            .GetByCampAndSubcontractorAsync(campId, dto.SubcontractorId);

        // Ensure the subcontractor is not already assigned for any of the services
        var duplicateServiceIds = subcontractorAssignments
            .Where(a => dto.ServiceIds.Contains(a.AssignmentId) && !a.IsDeleted)
            .Select(a => a.AssignmentId)
            .ToList();

        if (duplicateServiceIds.Any())
            throw new ValidationException([$"Subcontractor already assigned to some of these services: {string.Join(", ", duplicateServiceIds)}"]);

        // Ensure services being assigned belong to the camp package
        var allowedServiceIds = camp.PackageItems.Select(p => p.ReferenceId).ToHashSet();
        var invalidIds = dto.ServiceIds.Where(id => !allowedServiceIds.Contains(id)).ToList();
        if (invalidIds.Any())
            throw new ValidationException([$"Invalid services: {string.Join(", ", invalidIds)} are not part of this camp’s package."]);

        var pendingStatus = await _lookupSubcontractorHealthCampAssignmentRepository.FindByNameAsync("Pending");
        if (pendingStatus == null)
            throw new InvalidOperationException("Assignment status 'Pending' not found.");

        // 🔧 Helper to enforce UTC (solves PostgreSQL timestamp with time zone error)
        static DateTime ToUtc(DateTime value)
        {
            if (value.Kind == DateTimeKind.Utc)
                return value;
            if (value.Kind == DateTimeKind.Local)
                return value.ToUniversalTime();
            return DateTime.SpecifyKind(value, DateTimeKind.Utc);
        }

        foreach (var serviceId in dto.ServiceIds)
        {
            var referenceType = await _referenceResolver.ResolveTypeAsync(serviceId);
            var boothLabel = $"Booth-{Guid.NewGuid().ToString()[..4].ToUpper()}";

            var newAssignment = new SubcontractorHealthCampAssignment
            {
                Id = Guid.NewGuid(),
                HealthCampId = camp.Id,
                SubcontractorId = dto.SubcontractorId,
                AssignmentStatusId = pendingStatus.Id,
                BoothLabel = boothLabel,

                // FIX: ensure UTC datetime kind before saving
                StartDate = ToUtc(camp.StartDate),

                CreatedAt = DateTime.UtcNow,
                CreatedBy = actingUserId,
                IsDeleted = false,
                AssignmentId = serviceId,
                AssignmentType = (PackageItemType)referenceType
            };

            await _subcontractorCampAssignmentRepository.AddAsync(newAssignment);
        }

        await _notificationService.TriggerNotificationAsync(
            title: "Subcontractor Added to Camp",
            message: $"A subcontractor has been assigned to '{camp.Name}'.",
            type: "HealthCamp",
            entityId: camp.Id,
            entityType: "Camp",
            ct: ct
        );
    }



    public async Task RemoveSubcontractorFromCampAsync(Guid campId, Guid subcontractorId, Guid actingUserId)
    {
        var ct = CancellationToken.None;

        var assignments = await _subcontractorCampAssignmentRepository
            .GetByCampAndSubcontractorAsync(campId, subcontractorId);

        if (!assignments.Any())
            throw new NotFoundException("Subcontractor assignment", $"{subcontractorId} in camp {campId}");

        foreach (var a in assignments.Where(a => !a.IsDeleted))
        {
            a.IsDeleted = true;
            a.UpdatedAt = DateTime.UtcNow;
            a.UpdatedBy = actingUserId;
        }

        await _notificationService.TriggerNotificationAsync(
            title: "Subcontractor Removed from Camp",
            message: $"A subcontractor was removed from camp assignments.",
            type: "HealthCamp",
            entityId: campId,
            entityType: "Camp",
            ct: ct
        );

        await _subcontractorCampAssignmentRepository.SaveChangesAsync(ct);
    }


    public async Task AssignPackageToParticipantAsync(AssignParticipantPackageDto dto, CancellationToken ct)
    {
        // Verify participant belongs to camp and has billing info
        var participant = await _campParticipantRepository.GetParticipantWithBillingStatusAsync(dto.HealthCampId, dto.ParticipantId, ct)
            ?? throw new NotFoundException("Participant not found for this camp.");

        // Prevent assignment if billing not initiated
        if (participant.BillingStatus?.Name?.Equals("Not Billed", StringComparison.OrdinalIgnoreCase) == true)
            throw new ValidationException(["Cannot assign package until billing is initiated."]);

        // Get package through repository (no direct DbContext)
        var package = await _campPackageRepository.GetPackageByCampAsync(dto.HealthCampId, dto.HealthCampPackageId, ct)
            ?? throw new ValidationException(["The selected package does not belong to this camp."]);

        // Get existing active package
        var existing = await _participantPackageRepo.GetByParticipantIdAsync(dto.ParticipantId, ct);

        // Idempotency check
        if (existing != null && existing.HealthCampPackageId == dto.HealthCampPackageId && existing.IsActive)
            return; // nothing to change

        // Deactivate old package
        if (existing != null)
        {
            existing.IsActive = false;
            existing.UpdatedAt = DateTime.UtcNow;
            await _participantPackageRepo.UpdateAsync(existing, ct);
        }

        // Assign new package
        var newRecord = new HealthCampParticipantPackage
        {
            Id = Guid.NewGuid(),
            ParticipantId = dto.ParticipantId,
            HealthCampPackageId = dto.HealthCampPackageId,
            AssignedAt = DateTime.UtcNow,
            IsActive = true
        };

        await _participantPackageRepo.AddAsync(newRecord, ct);
    }

    public async Task<List<HealthCampPackageDto>> GetAllPackagesByCampAsync(Guid campId, CancellationToken ct)
    {
        var packages = await _campPackageRepository.GetAllPackagesWithServicesByCampAsync(campId, ct);

        // Map manually or via AutoMapper
        return [.. packages.Select(p => new HealthCampPackageDto
        {
            Id = p.Id,
            HealthCampId = p.HealthCampId,
            ServicePackageId = p.ServicePackageId,
            ServicePackageName = p.ServicePackage?.Name,
        })];
    }




}