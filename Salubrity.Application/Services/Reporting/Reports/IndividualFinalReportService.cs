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

        // 5. Body-map entries — worst status + clinical conclusion per service section.
        var bodyMap = prelim.ServiceSections.Select(section =>
        {
            var worst = WorstStatus(section.Metrics.Select(m => m.Status));
            return new BodyMapEntryDto
            {
                ServiceName = section.ServiceName,
                IconKey = section.IconKey,
                Status = worst,
                Conclusion = DeriveConclusion(section.ServiceName, section.Metrics),
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


    /// <summary>
    /// Turn a section's metric labels (plus the service name as fallback) into a clinical
    /// conclusion shown on the body map — e.g. "Hypertension" instead of "Abnormal".
    /// Conservative: only labels with a clinically standard term are mapped.
    /// </summary>
    private static string DeriveConclusion(string serviceName, IReadOnlyList<MetricDto> metrics)
    {
        var sname = (serviceName ?? string.Empty).ToLowerInvariant();

        // Look for the worst metric first (Abnormal beats Borderline) — its label drives the term.
        var abnormal = metrics.FirstOrDefault(m => string.Equals(m.Status, "Abnormal", StringComparison.OrdinalIgnoreCase));
        var borderline = metrics.FirstOrDefault(m => string.Equals(m.Status, "Borderline", StringComparison.OrdinalIgnoreCase));
        var driver = abnormal ?? borderline;
        if (driver == null) return "Normal";

        var lcLabel = (driver.Label ?? string.Empty).ToLowerInvariant();
        var isAbnormal = abnormal != null;

        // Metric-label-driven mapping (most specific, kicks in regardless of service).
        if (lcLabel.Contains("blood pressure") || lcLabel.StartsWith("bp")) return "Hypertension";
        if (lcLabel.Contains("heart rate") || lcLabel.Contains("pulse")) return "Abnormal heart rate";
        if (lcLabel.Contains("bmi") || lcLabel.Contains("body mass")) return isAbnormal ? "Obesity" : "Overweight";
        if (lcLabel.Contains("random blood sugar") || lcLabel.Contains("rbs") || lcLabel.Contains("blood sugar") || lcLabel.Contains("glucose"))
            return "Hyperglycemia";
        if (lcLabel.Contains("hba1c")) return "Elevated HbA1c";
        if (lcLabel.Contains("cholesterol")) return "Dyslipidemia";
        if (lcLabel.Contains("temperature")) return "Fever";
        if (lcLabel.Contains("oxygen") || lcLabel.Contains("spo")) return "Low oxygen saturation";
        if (lcLabel.Contains("creatinine") || lcLabel.Contains("urea")) return "Renal concern";
        if (lcLabel.Contains("ast") || lcLabel.Contains("alt") || lcLabel.Contains("ggt") || lcLabel.Contains("bilirubin"))
            return "Hepatic concern";

        // Service-name fallback (when label is non-numeric / generic like "Notes").
        if (sname.Contains("mental")) return "Mental health concern";
        if (sname.Contains("vision") || sname.Contains("eye") || sname.Contains("visual")) return "Visual impairment";
        if (sname.Contains("ent") || sname.Contains("ear, nose") || sname.Contains("nose")) return "ENT condition";
        if (sname.Contains("dental")) return "Dental issue";
        if (sname.Contains("mss") || sname.Contains("musculoskeletal") || sname.Contains("physiotherap") || sname.Contains("joint"))
            return "Musculoskeletal concern";
        if (sname.Contains("nutrition")) return "Nutritional concern";
        if (sname.Contains("lab")) return "Abnormal lab result";
        if (sname.Contains("well woman") || sname.Contains("breast")) return "Women's health concern";
        if (sname.Contains("audiomet")) return "Hearing concern";
        if (sname.Contains("ultrasound")) return "Imaging finding";
        if (sname.Contains("lung")) return "Pulmonary concern";

        return isAbnormal ? "Abnormal" : "Borderline";
    }
}
