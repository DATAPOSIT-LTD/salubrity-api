using AutoMapper;
using Microsoft.Extensions.Logging;
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
using Salubrity.Domain.Common;
using Salubrity.Domain.Entities.HealthCamps;
using Salubrity.Domain.Entities.HealthcareServices;
using Salubrity.Domain.Entities.Join;
using Salubrity.Domain.Entities.Lookup;
using Salubrity.Domain.Entities.Subcontractor;
using Salubrity.Shared.Exceptions;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Salubrity.Application.Services.HealthCamps;

public sealed class CampTokenOptions
{
    public string AppBaseUrl { get; set; } = "https://app.salubritycentre.com/register";
    public string Audience { get; set; } = "camp-signin";
    public string Issuer { get; set; } = "salubrity-api";
}
public class HealthCampService : IHealthCampService
{
    private readonly IHealthCampRepository _repo;
    private readonly ILogger<HealthCampService> _logger;

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
    private readonly CampTokenOptions _campTokenOptions;


    public HealthCampService(ILogger<HealthCampService> logger, IHealthCampPackageRepository campPackageRepository, IHealthCampRepository repo, ILookupRepository<HealthCampStatus> lookupRepository, IPackageReferenceResolver _pResolver, IMapper mapper, ICampTokenFactory tokenFactory, IEmailService emailService, IQrCodeService qrCodeService, ITempPasswordService tempPasswordService, IEmployeeReadRepository employeeReadRepo, IFileStorage files, ISubcontractorCampAssignmentRepository subcontractorCampAssignment, ILookupRepository<SubcontractorHealthCampAssignmentStatus> lookupSubcontractorHealthCampAssignmentRepository, INotificationService notificationService, IHealthCampParticipantRepository campParticipantRepository, IJwtService jwt, IRoleRepository roleRepository, IHealthCampParticipantPackageRepository participantPackageRepo)
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
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
        //  Helper for UTC-safe conversion
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

        if (string.IsNullOrWhiteSpace(entity.Slug))
        {
            entity.Slug = SlugHelper.Generate(
                entity.Name,
                entity.StartDate.Year
            );
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
        var assignedStatus = await _lookupSubcontractorHealthCampAssignmentRepository.FindByNameAsync("Pending") ?? throw new InvalidOperationException("Assignment status 'Pending' not found");
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

                // Always store UTC values for PostgreSQL
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

        // Basic field updates
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

        // Handle updated packages if provided
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
        _logger.LogInformation("Launching health camp {@Dto}", dto);

        try
        {
            var camp = await _repo.GetForLaunchAsync(dto.HealthCampId)
                       ?? throw new NotFoundException("Camp not found");
            _logger.LogInformation("Loaded camp {CampId} - {CampName}", camp.Id, camp.Name);

            if (string.IsNullOrWhiteSpace(camp.Slug))
            {
                // self-heal legacy data
                camp.Slug = SlugHelper.Generate(
                    camp.Name,
                    camp.StartDate.Year
                );

                await _repo.UpdateAsync(camp);

                _logger.LogInformation(
                    "Generated missing slug for camp {CampId}: {Slug}",
                    camp.Id,
                    camp.Slug
                );
            }


            if (camp.HealthCampStatus == null)
                throw new InvalidOperationException("Camp status is missing.");

            var upcomingStatus = await _lookupRepository.FindByNameAsync("Upcoming")
                ?? throw new InvalidOperationException("'Upcoming' status not found");

            if (camp.HealthCampStatusId != upcomingStatus.Id)
                throw new ValidationException(["Only camps in 'Upcoming' status can be launched."]);

            // Timezone
            TimeZoneInfo eat;
            try
            {
                eat = TimeZoneInfo.FindSystemTimeZoneById("Africa/Nairobi");
            }
            catch (TimeZoneNotFoundException)
            {
                eat = TimeZoneInfo.Utc;
                _logger.LogWarning("Timezone 'Africa/Nairobi' not found; using UTC instead");
            }

            var nowUtc = DateTime.UtcNow;
            var todayLocal = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, eat).Date;
            _logger.LogInformation("Launching at {LocalTime}", todayLocal);

            var startDate = camp.StartDate.Date;
            var endDate = (camp.EndDate ?? camp.StartDate).Date;

            if (todayLocal < startDate)
                throw new ValidationException([$"You can only launch this camp on or after its start date: {startDate:dd MMM yyyy}."]);

            if (todayLocal > endDate)
                throw new ValidationException([$"This camp already ended on {endDate:dd MMM yyyy} and cannot be launched."]);

            var closeUtc = dto.CloseDate.ToUniversalTime();
            _logger.LogInformation("Close date set to {CloseUtc}", closeUtc);

            await _notificationService.TriggerNotificationAsync(
                title: "Health Camp Launched",
                message: $"Health camp '{camp.Name}' has been launched.",
                type: "HealthCamp",
                entityId: camp.Id,
                entityType: "Camp",
                ct: ct
            );

            // Assign new JTIs (security anchors)
            camp.ParticipantPosterJti = Guid.NewGuid().ToString("N");
            camp.SubcontractorPosterJti = Guid.NewGuid().ToString("N");
            camp.PosterTokensExpireAt = closeUtc;

            var participantRole = await _roleRepository.FindByNameAsync("participant")
                ?? await _roleRepository.FindByNameAsync("patient");
            var subcontractorRole = await _roleRepository.FindByNameAsync("subcontractor");

            if (participantRole == null || subcontractorRole == null)
                throw new InvalidOperationException("Role not found.");

            // ─────────────────────────────────────────────
            // PUBLIC, HUMAN-FRIENDLY POSTER URLS (NO JWT)
            // ─────────────────────────────────────────────
            var publicBaseUrl = "https://app.salubritycentre.com/register";// _campTokenOptions.AppBaseUrl.TrimEnd('/');

            var participantPosterUrl =
                $"{publicBaseUrl}/health-camp/{camp.Slug}/participant";

            var subcontractorPosterUrl =
                $"{publicBaseUrl}/health-camp/{camp.Slug}/subcontractor";

            var patientQrBase64 = _qr.GenerateBase64Png(participantPosterUrl);
            var subcoQrBase64 = _qr.GenerateBase64Png(subcontractorPosterUrl);

            _logger.LogInformation("Generated poster QR codes for participants and subcontractors");

            // Skip email sending for now
            _logger.LogInformation("Email service disabled — skipping participant and subcontractor invites");

            // Finalize status
            var ongoingStatus = await _lookupRepository.FindByNameAsync("Ongoing")
                                 ?? throw new InvalidOperationException("Ongoing status not found");

            camp.HealthCampStatusId = ongoingStatus.Id;
            camp.IsLaunched = true;
            camp.CloseDate = closeUtc;

            await _repo.UpdateAsync(camp);
            _logger.LogInformation("Updated camp status to Ongoing");

            // Save QR PNGs for dashboard posters
            var folder = $"qrcodes/healthcamps/{camp.Id:N}";
            var participantBytes = DecodeBase64Png(patientQrBase64);
            var subcontractorBytes = DecodeBase64Png(subcoQrBase64);

            var participantFile = $"participant_{camp.ParticipantPosterJti}_{closeUtc:yyyyMMddHHmmss}.png";
            var subcontractorFile = $"subcontractor_{camp.SubcontractorPosterJti}_{closeUtc:yyyyMMddHHmmss}.png";

            var participantPngUrl = await _files.SaveAsync(participantBytes, folder, participantFile, "image/png");
            var subcontractorPngUrl = await _files.SaveAsync(subcontractorBytes, folder, subcontractorFile, "image/png");

            _logger.LogInformation("Saved QR codes to {Folder}", folder);

            return new LaunchHealthCampResponseDto
            {
                HealthCampId = camp.Id,
                CloseDate = closeUtc,
                ParticipantPosterQrUrl = participantPngUrl,
                SubcontractorPosterQrUrl = subcontractorPngUrl
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error launching camp {CampId}: {Message}", dto.HealthCampId, ex.Message);
            throw;
        }
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

    public async Task<List<HealthCampListDto>> GetMyUpcomingCampsAsync(
      Guid? subcontractorId,
      CancellationToken ct = default)
    {
        var camps = await _repo.GetUpcomingCampsAsync(subcontractorId, ct);
        return _mapper.Map<List<HealthCampListDto>>(camps);
    }


    public async Task<List<HealthCampListDto>> GetMyOngoingCampsAsync(
        Guid? subcontractorId,
        CancellationToken ct = default)
    {
        var camps = await _repo.GetOngoingCampsAsync(subcontractorId, ct);
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


    public async Task<List<HealthCampWithRolesDto>>
    GetMyCampsWithRolesByStatusAsync(
        Guid? subcontractorId,
        string status,
        CancellationToken ct = default)
    {
        if (!sourceArray.Contains(status))
            throw new ValidationException(["Invalid camp status filter."]);

        var camps = await _repo.GetCampsWithRolesByStatusAsync(
            subcontractorId,
            status,
            ct);

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


    public async Task<CampLinkResultDto> LinkUserToCampAsync(
     Guid userId,
     Guid campId,
     CancellationToken ct = default)
    {
        var result = new CampLinkResultDto
        {
            CampId = campId
        };

        // ─────────────────────────────────────────────
        // LOAD CAMP
        // ─────────────────────────────────────────────
        var camp = await _repo.GetByIdAsync(campId);
        if (camp is null)
        {
            result.Warnings.Add("Camp not found.");
            return result;
        }

        // ─────────────────────────────────────────────
        // VALIDATE CAMP STATE
        // ─────────────────────────────────────────────
        var today = DateTime.UtcNow.Date;

        if (!camp.IsLaunched)
        {
            result.Warnings.Add("Camp is not active.");
            return result;
        }

        if (camp.EndDate.HasValue && camp.EndDate.Value.Date < today)
        {
            result.Warnings.Add("Camp has already ended.");
            return result;
        }

        // ─────────────────────────────────────────────
        // IDEMPOTENT PARTICIPANT LINKING
        // ─────────────────────────────────────────────
        var alreadyLinked =
            await _campParticipantRepository
                .IsParticipantLinkedToCampAsync(campId, userId, ct);

        if (alreadyLinked)
        {
            result.Linked = true;
            result.Info.Add("User already registered for this camp.");
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
        result.Info.Add("User registered for camp.");

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

    public async Task<QrEncodingDetailDto> DecodePosterTokenAsync(
     string token,
     CancellationToken ct)
    {
        // Cryptographically validate token (issuer, audience, expiry, signature)
        var principal = _jwt.ValidateToken(token);

        var claims = principal.Claims.ToList();

        // ─────────────────────────────────────────────
        // REQUIRED CLAIMS
        // ─────────────────────────────────────────────

        // campId MUST come from token
        var campIdClaim = claims.FirstOrDefault(c => c.Type == "campId")?.Value;
        if (!Guid.TryParse(campIdClaim, out var campId))
            throw new ValidationException(["Invalid or missing campId in poster token."]);

        // role MUST be present
        var role = claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
        if (string.IsNullOrWhiteSpace(role))
            throw new ValidationException(["Invalid or missing role in poster token."]);

        // poster flag MUST be present
        var isPoster = claims.Any(c => c.Type == "poster" && c.Value == "1");
        if (!isPoster)
            throw new ValidationException(["Token is not a poster token."]);

        // jti is optional but useful
        var jti = claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value
                  ?? claims.FirstOrDefault(c => c.Type == "jti")?.Value;

        // expiry (already validated, but returned for UI/debug)
        var expClaim = claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Exp)?.Value;
        var expiresAt = expClaim != null
            ? DateTimeOffset.FromUnixTimeSeconds(long.Parse(expClaim))
            : DateTimeOffset.MinValue;

        return new QrEncodingDetailDto
        {
            Token = token,
            CampId = campId,
            Role = role,
            IsPoster = true,
            Jti = jti,
            ExpiresAt = expiresAt
        };
    }

    public async Task AddSubcontractorToCampAsync(
        Guid campId,
        ModifySubcontractorCampDto dto,
        Guid actingUserId)
    {
        var ct = CancellationToken.None;

        // ─────────────────────────────────────────────
        // 1. Load camp (same as CreateAsync persistence target)
        // ─────────────────────────────────────────────
        var camp = await _repo.GetByIdAsync(campId)
            ?? throw new NotFoundException("Health Camp", campId.ToString());

        // ─────────────────────────────────────────────
        // 2. Guard: assignments required (CreateAsync implicit invariant)
        // ─────────────────────────────────────────────
        if (dto.Assignments == null || !dto.Assignments.Any())
        {
            throw new ValidationException([
                "At least one service assignment is required."
            ]);
        }

        // ─────────────────────────────────────────────
        // 3. Guard: NO EMPTY SERVICE IDS (this is the missing invariant)
        // ─────────────────────────────────────────────
        var emptyServiceIds = dto.Assignments
            .Where(a => a.ServiceId == Guid.Empty)
            .Select(a => a.ServiceId)
            .ToList();

        if (emptyServiceIds.Any())
        {
            throw new ValidationException([
                "ServiceId cannot be empty."
            ]);
        }

        // ─────────────────────────────────────────────
        // 4. Validate services belong to camp package
        //    (EXACTLY matches CreateAsync semantics)
        // ─────────────────────────────────────────────
        var allowedServiceIds = camp.PackageItems
            .Select(p => p.ReferenceId)
            .ToHashSet();

        var invalidServices = dto.Assignments
            .Where(a => !allowedServiceIds.Contains(a.ServiceId))
            .Select(a => a.ServiceId)
            .Distinct()
            .ToList();

        if (invalidServices.Any())
        {
            throw new ValidationException([
                $"Invalid services not part of this camp package: {string.Join(", ", invalidServices)}"
            ]);
        }

        // ─────────────────────────────────────────────
        // 5. Duplicate protection (service-level only)
        //    (matches booth creation rules in CreateAsync)
        // ─────────────────────────────────────────────
        var existingAssignments =
            await _subcontractorCampAssignmentRepository
                .GetByCampAndSubcontractorAsync(campId, dto.SubcontractorId);

        var duplicateServiceIds = existingAssignments
            .Where(x => !x.IsDeleted)
            .Select(x => x.AssignmentId)
            .Intersect(dto.Assignments.Select(a => a.ServiceId))
            .ToList();

        if (duplicateServiceIds.Any())
        {
            throw new ValidationException([
                $"Subcontractor already assigned to services: {string.Join(", ", duplicateServiceIds)}"
            ]);
        }

        // ─────────────────────────────────────────────
        // 6. Resolve assignment status (same as CreateAsync)
        // ─────────────────────────────────────────────
        var assignedStatus =
            await _lookupSubcontractorHealthCampAssignmentRepository
                .FindByNameAsync("Pending")
            ?? throw new InvalidOperationException(
                "Assignment status 'Pending' not found."
            );

        // ─────────────────────────────────────────────
        // 7. UTC helper (byte-for-byte equivalent)
        // ─────────────────────────────────────────────
        static DateTime ToUtc(DateTime value)
        {
            if (value.Kind == DateTimeKind.Utc)
                return value;
            if (value.Kind == DateTimeKind.Local)
                return value.ToUniversalTime();
            return DateTime.SpecifyKind(value, DateTimeKind.Utc);
        }

        // ─────────────────────────────────────────────
        // 8. Create booth assignments (OPERATIONAL layer)
        //    EXACT MATCH to CreateAsync loop
        // ─────────────────────────────────────────────
        foreach (var assignment in dto.Assignments)
        {
            var referenceType =
                await _referenceResolver.ResolveTypeAsync(assignment.ServiceId);

            var boothAssignment = new SubcontractorHealthCampAssignment
            {
                Id = Guid.NewGuid(),
                HealthCampId = camp.Id,
                SubcontractorId = dto.SubcontractorId,
                AssignmentStatusId = assignedStatus.Id,
                BoothLabel = $"Booth-{Guid.NewGuid().ToString()[..4].ToUpper()}",

                StartDate = ToUtc(camp.StartDate),
                CreatedAt = DateTime.UtcNow,
                CreatedBy = actingUserId,

                IsDeleted = false,
                IsPrimaryAssignment = true,

                AssignmentId = assignment.ServiceId,
                AssignmentType = (PackageItemType)referenceType
            };

            await _subcontractorCampAssignmentRepository.AddAsync(boothAssignment);
        }

        // ─────────────────────────────────────────────
        // 9. Notification (unchanged)
        // ─────────────────────────────────────────────
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