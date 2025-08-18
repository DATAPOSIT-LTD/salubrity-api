using AutoMapper;
using Salubrity.Application.DTOs.Email;
using Salubrity.Application.DTOs.HealthCamps;
using Salubrity.Application.Interfaces;
using Salubrity.Application.Interfaces.Repositories.HealthCamps;
using Salubrity.Application.Interfaces.Repositories.Lookups;
using Salubrity.Application.Interfaces.Services.HealthCamps;
using Salubrity.Application.Interfaces.Services.HealthcareServices;
using Salubrity.Domain.Entities.HealthCamps;
using Salubrity.Domain.Entities.HealthcareServices;
using Salubrity.Domain.Entities.Lookup;
using Salubrity.Shared.Exceptions;
using System.Text.Json.Serialization;

namespace Salubrity.Application.Services.HealthCamps;

public class HealthCampService : IHealthCampService
{
    private readonly IHealthCampRepository _repo;
    private readonly ILookupRepository<HealthCampStatus> _lookupRepository;
    private readonly IMapper _mapper;
    private readonly IPackageReferenceResolver _referenceResolver;
    private readonly ICampTokenFactory _tokenFactory;

    private readonly IQrCodeService _qr;
    private readonly ITempPasswordService _tempPassword;
    private readonly IEmailService _email;

    public HealthCampService(IHealthCampRepository repo, ILookupRepository<HealthCampStatus> lookupRepository, IPackageReferenceResolver _pResolver, IMapper mapper, ICampTokenFactory tokenFactory, IEmailService emailService, IQrCodeService qrCodeService, ITempPasswordService tempPasswordService)
    {
        _repo = repo;
        _mapper = mapper;
        _referenceResolver = _pResolver;
        _lookupRepository = lookupRepository;
        _tokenFactory = tokenFactory ?? throw new ArgumentNullException(nameof(tokenFactory));
        _email = emailService;
        _qr = qrCodeService;
        _tempPassword = tempPasswordService;
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
            PackageItems = [],
            ExpectedParticipants = dto.ExpectedParticipants,
            HealthCampStatusId = upcomingStatus.Id,
            ServiceAssignments = []
        };


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

        foreach (var assignment in dto.ServiceAssignments)
        {
            entity.ServiceAssignments.Add(new HealthCampServiceAssignment
            {
                Id = Guid.NewGuid(),
                HealthCampId = entity.Id,
                ServiceId = assignment.ServiceId,
                SubcontractorId = assignment.SubcontractorId
            });
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
                Subject = "Health Camp Invitation",
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
                Subject = "Health Camp Invitation",
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

        if (camp.HealthCampStatus != null)
        {
            camp.HealthCampStatus.Name = "Ongoing";
        }

        // Persist all changes
        await _repo.UpdateAsync(camp);
        await _repo.SaveChangesAsync();

        // Poster QR codes for admin printing
        var participantPosterToken = _tokenFactory.CreatePosterToken(camp.Id, "participant", camp.ParticipantPosterJti!, closeUtc);
        var subcontractorPosterToken = _tokenFactory.CreatePosterToken(camp.Id, "subcontractor", camp.SubcontractorPosterJti!, closeUtc);

        var patientQrBase64 = _qr.GenerateBase64Png(_tokenFactory.BuildSignInUrl(participantPosterToken));
        var subcoQrBase64 = _qr.GenerateBase64Png(_tokenFactory.BuildSignInUrl(subcontractorPosterToken));

        return new LaunchHealthCampResponseDto
        {
            HealthCampId = camp.Id,
            CloseDate = closeUtc,
            ParticipantPosterQrBase64 = patientQrBase64,
            SubcontractorPosterQrBase64 = subcoQrBase64
        };
    }


    public async Task DeleteAsync(Guid id)
    {
        var camp = await _repo.GetByIdAsync(id) ?? throw new NotFoundException("Camp not found");
        await _repo.DeleteAsync(camp.Id);
    }
}