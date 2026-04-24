using QuestPDF.Fluent;
// File: Application/Services/Reporting/Reports/IndividualFinalReportService.cs
using Salubrity.Application.Common.Interfaces.Repositories;
using Salubrity.Application.DTOs.Lookups;
using Salubrity.Application.DTOs.Reports;
using Salubrity.Application.Interfaces.Repositories.Clinical;
using Salubrity.Application.Interfaces.Services.Clinical;
using Salubrity.Application.Interfaces.Services.Reporting;
using Salubrity.Application.Interfaces.Repositories.Users;
using Salubrity.Shared.Exceptions;

namespace Salubrity.Application.Services.Reporting.Reports;

/// <summary>
/// Builds the Individual Final Report payload.
/// Composes the Preliminary report and adds doctor recommendations, referrals,
/// body-map entries (per-service worst status), and a signature block.
/// </summary>
public sealed class IndividualFinalReportService : IIndividualFinalReportService
{
    private readonly IIndividualPreliminaryReportService _preliminary;
    private readonly IHealthCampParticipantRepository _participantRepo;
    private readonly IDoctorRecommendationRepository _recommendationRepo;
    private readonly IServiceReferralService _referralService;
    private readonly IUserRepository _userRepo;

    public IndividualFinalReportService(
        IIndividualPreliminaryReportService preliminary,
        IHealthCampParticipantRepository participantRepo,
        IDoctorRecommendationRepository recommendationRepo,
        IServiceReferralService referralService,
        IUserRepository userRepo)
    {
        _preliminary = preliminary;
        _participantRepo = participantRepo;
        _recommendationRepo = recommendationRepo;
        _referralService = referralService;
        _userRepo = userRepo;
    }

    public async Task<IndividualFinalReportDto> BuildAsync(Guid participantId, CancellationToken ct = default)
    {
        // 1. Start from the Preliminary payload — covers demographics, KPIs, findings, risk bars.
        var prelim = await _preliminary.BuildAsync(participantId, ct);

        // 2. Resolve patient + camp (DoctorRecommendation.PatientId and ServiceReferral.ParticipantId
        //    both store Patient.Id, not HealthCampParticipant.Id).
        var participant = await _participantRepo.GetParticipantWithDemographicsAsync(participantId, ct)
            ?? throw new NotFoundException("Participant not found.");

        if (participant.PatientId is null)
            throw new ValidationException(new List<string> { "Participant is not linked to a patient profile." });

        var patientId = participant.PatientId.Value;
        var campId = participant.HealthCampId;

        // 3. Latest doctor recommendation for this patient+camp (may be null).
        var allRecs = await _recommendationRepo.GetByPatientAsync(patientId, ct);
        var latestRec = allRecs
            .Where(r => r.HealthCampId == campId)
            .OrderByDescending(r => r.CreatedAt)
            .FirstOrDefault();

        FinalReportDoctorRecommendationDto? recommendationDto = null;
        string preparedByName = string.Empty;
        Guid? preparedById = null;
        DateTime? preparedAt = null;

        if (latestRec is not null)
        {
            var doctor = await _userRepo.FindUserByIdAsync(latestRec.DoctorId);
            var doctorName = doctor is null ? string.Empty : doctor.FullName;

            recommendationDto = new FinalReportDoctorRecommendationDto
            {
                Id = latestRec.Id,
                DoctorId = latestRec.DoctorId,
                DoctorName = doctorName,
                PertinentHistoryFindings = latestRec.PertinentHistoryFindings,
                PertinentClinicalFindings = latestRec.PertinentClinicalFindings,
                DiagnosticImpression = latestRec.DiagnosticImpression,
                Conclusion = latestRec.Conclusion,
                Instructions = latestRec.Instructions,
                FollowUpRecommendation = latestRec.FollowUpRecommendation is null
                    ? null
                    : new BaseLookupResponse { Id = latestRec.FollowUpRecommendation.Id, Name = latestRec.FollowUpRecommendation.Name },
                RecommendationType = latestRec.RecommendationType is null
                    ? null
                    : new BaseLookupResponse { Id = latestRec.RecommendationType.Id, Name = latestRec.RecommendationType.Name },
                CreatedAt = latestRec.CreatedAt,
            };

            preparedByName = doctorName;
            preparedById = latestRec.DoctorId;
            preparedAt = latestRec.CreatedAt;
        }

        // 4. Referrals raised at service stations.
        var referrals = await _referralService.GetByParticipantAndCampAsync(patientId, campId, ct);
        var referralDtos = referrals.Select(r => new FinalReportReferralDto
        {
            Id = r.Id,
            ServiceAssignmentId = r.ServiceAssignmentId,
            ServiceName = r.ServiceName,
            ServiceProviderName = r.ServiceProviderName,
            Speciality = r.Speciality,
            Reason = r.Reason,
            Urgency = r.Urgency,
            FollowUpSchedule = r.FollowUpSchedule,
            CreatedAt = r.CreatedAt,
        }).ToList();

        // 5. Body-map entries — worst status across each service section's metrics.
        var bodyMap = prelim.ServiceSections.Select(section =>
        {
            var worst = WorstStatus(section.Metrics.Select(m => m.Status));
            return new BodyMapEntryDto
            {
                ServiceName = section.ServiceName,
                IconKey = section.IconKey,
                Status = worst,
            };
        }).ToList();

        // 6. Compose the Final DTO by projecting all Preliminary fields onto the subtype.
        return new IndividualFinalReportDto
        {
            Demographics = prelim.Demographics,
            CampName = prelim.CampName,
            CampDate = prelim.CampDate,
            GeneratedAt = prelim.GeneratedAt,
            ResultsAtGlance = prelim.ResultsAtGlance,
            AbnormalFindings = prelim.AbnormalFindings,
            BorderlineFindings = prelim.BorderlineFindings,
            ServiceSections = prelim.ServiceSections,
            GeneralHealthScore = prelim.GeneralHealthScore,
            RiskBars = prelim.RiskBars,

            DoctorRecommendation = recommendationDto,
            Referrals = referralDtos,
            BodyMap = bodyMap,
            Signature = new ReportSignatureDto
            {
                PreparedById = preparedById,
                PreparedByName = preparedByName,
                PreparedAt = preparedAt,
            },
        };
    }


    public async Task<byte[]> BuildPdfAsync(Guid participantId, CancellationToken ct = default)
    {
        var dto = await BuildAsync(participantId, ct);
        var doc = new IndividualFinalReportDocument(dto);
        return doc.GeneratePdf();
    }

    private static string WorstStatus(IEnumerable<string> statuses)
    {
        var rank = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["Normal"] = 0,
            ["Borderline"] = 1,
            ["Abnormal"] = 2,
        };
        var worst = 0;
        foreach (var s in statuses)
        {
            if (rank.TryGetValue(s ?? "Normal", out var v) && v > worst) worst = v;
        }
        return worst switch { 2 => "Abnormal", 1 => "Borderline", _ => "Normal" };
    }
}
