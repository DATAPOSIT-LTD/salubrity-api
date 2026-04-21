using QuestPDF.Fluent;
using Salubrity.Application.Common.Interfaces.Repositories;
using Salubrity.Application.DTOs.Reports;
using Salubrity.Application.Interfaces.Repositories.IntakeForms;
using Salubrity.Application.Interfaces.Services.Reporting;
using Salubrity.Shared.Exceptions;

namespace Salubrity.Application.Services.Reporting.Reports;

/// <summary>
/// Builds the Individual Preliminary Report payload for a participant in a completed camp.
/// All vital classification, scoring, and risk-band logic is delegated to the helper classes
/// (VitalThresholds, HealthScoreCalculator, RiskBandMapper) so this service is mostly orchestration.
/// </summary>
public sealed class IndividualPreliminaryReportService : IIndividualPreliminaryReportService
{
    private readonly IHealthCampParticipantRepository _participantRepo;
    private readonly IIntakeFormResponseRepository _responseRepo;

    public IndividualPreliminaryReportService(
        IHealthCampParticipantRepository participantRepo,
        IIntakeFormResponseRepository responseRepo)
    {
        _participantRepo = participantRepo;
        _responseRepo = responseRepo;
    }

    public async Task<IndividualPreliminaryReportDto> BuildAsync(Guid participantId, CancellationToken ct = default)
    {
        // ── 1. Resolve participant → patient + camp ──────────────────────────
        var participant = await _participantRepo.GetParticipantWithDemographicsAsync(participantId, ct)
            ?? throw new NotFoundException("Participant not found.");

        if (participant.PatientId is null)
            throw new ValidationException(new List<string> { "Participant is not linked to a patient profile." });

        var patientId = participant.PatientId.Value;
        var campId = participant.HealthCampId;

        // ── 2. Demographics ──────────────────────────────────────────────────
        var demographics = new PatientDemographicsDto
        {
            FullName = $"{participant.User.FirstName} {participant.User.LastName}".Trim(),
            Gender = participant.User.Gender?.Name ?? "—",
            DateOfBirth = participant.User.DateOfBirth,
            Phone = participant.User.Phone ?? "—",
            Email = participant.User.Email,
            Branch = participant.Patient?.PrimaryOrganization?.BusinessName ?? "—",
            Nationality = "—" // not currently stored on the user/patient record
        };

        // ── 3. Load all submitted intake form responses for this patient+camp ─
        var responses = await _responseRepo.GetResponsesByPatientAndCampIdAsync(patientId, campId, ct);

        // ── 4. Build per-service sections + aggregate counts ─────────────────
        var serviceSections = new List<ServiceSectionDto>();
        var abnormalFindings = new List<string>();
        var borderlineFindings = new List<string>();
        var normalCount = 0;
        var borderlineCount = 0;
        var abnormalCount = 0;

        // Hold latest numeric values for the risk bars
        decimal? latestBmi = null;
        decimal? latestSystolic = null;
        decimal? latestGlucose = null;
        decimal? latestCholesterol = null;

        foreach (var responseGroup in responses
                     .GroupBy(r => new { r.ServiceId, ServiceName = r.Service?.Name ?? r.Version.IntakeFormName })
                     .OrderBy(g => g.Key.ServiceName))
        {
            var latest = responseGroup.OrderByDescending(r => r.CreatedAt).First();

            var section = new ServiceSectionDto
            {
                ServiceName = responseGroup.Key.ServiceName ?? "Service",
                IconKey = ResolveIconKey(responseGroup.Key.ServiceName),
                Summary = string.Empty
            };

            foreach (var fr in latest.FieldResponses
                         .Where(fr => !string.IsNullOrWhiteSpace(fr.Value))
                         .OrderBy(fr => fr.Field?.Order ?? 0))
            {
                var label = fr.Field?.Label ?? "Field";
                var value = fr.Value!.Trim();
                var fieldType = (fr.Field?.FieldType ?? "").ToLowerInvariant();

                // Blood pressure stored as "120/80"
                if (fieldType == "blood-pressure" || label.ToLowerInvariant().Contains("blood pressure"))
                {
                    var bpStatus = VitalThresholds.ClassifyBloodPressure(value);
                    if (bpStatus is null) continue;

                    section.Metrics.Add(new MetricDto
                    {
                        Label = label,
                        Value = $"{value} mmHg",
                        Status = bpStatus.Value.ToString()
                    });
                    Tally(bpStatus.Value, label, value, "mmHg",
                        ref normalCount, ref borderlineCount, ref abnormalCount,
                        abnormalFindings, borderlineFindings);

                    if (value.Contains('/') &&
                        decimal.TryParse(value.Split('/')[0].Trim(), out var sys))
                        latestSystolic = sys;

                    continue;
                }

                if (decimal.TryParse(value, out var num))
                {
                    var status = VitalThresholds.Classify(label, num);

                    section.Metrics.Add(new MetricDto
                    {
                        Label = label,
                        Value = value,
                        Status = status.ToString()
                    });
                    Tally(status, label, value, "",
                        ref normalCount, ref borderlineCount, ref abnormalCount,
                        abnormalFindings, borderlineFindings);

                    var lower = label.ToLowerInvariant();
                    if (lower.Contains("bmi") || lower.Contains("body mass")) latestBmi = num;
                    else if (lower.Contains("glucose") || lower.Contains("blood sugar")) latestGlucose = num;
                    else if (lower.Contains("cholesterol")) latestCholesterol = num;

                    continue;
                }

                // Non-numeric fields (e.g. checklist findings like "CVS: Normal").
                // Show them and count as Normal so the KPI cards reflect actual data.
                section.Metrics.Add(new MetricDto
                {
                    Label = label,
                    Value = value,
                    Status = "Normal"
                });
                normalCount++;
            }

            serviceSections.Add(section);
        }

        // ── 5. Compose the final DTO ─────────────────────────────────────────
        var score = HealthScoreCalculator.Compute(normalCount, borderlineCount, abnormalCount);

        return new IndividualPreliminaryReportDto
        {
            Demographics = demographics,
            CampName = participant.HealthCamp.Name,
            GeneratedAt = DateTime.UtcNow,
            ResultsAtGlance = new ResultsAtGlanceDto
            {
                ParametersTested = normalCount + borderlineCount + abnormalCount,
                NormalCount = normalCount,
                BorderlineCount = borderlineCount,
                AbnormalCount = abnormalCount
            },
            AbnormalFindings = abnormalFindings,
            BorderlineFindings = borderlineFindings,
            ServiceSections = serviceSections,
            GeneralHealthScore = new GeneralHealthScoreDto
            {
                Score = score.Score,
                Message = score.Message
            },
            RiskBars = BuildRiskBars(latestBmi, latestSystolic, latestGlucose, latestCholesterol)
        };
    }

    public async Task<byte[]> BuildPdfAsync(Guid participantId, CancellationToken ct = default)
    {
        var dto = await BuildAsync(participantId, ct);
        var doc = new IndividualPreliminaryReportDocument(dto, "Individual Preliminary Report");
        return doc.GeneratePdf();
    }

    private static void Tally(
        VitalStatus status, string label, string value, string unit,
        ref int normal, ref int borderline, ref int abnormal,
        List<string> abnormalList, List<string> borderlineList)
    {
        switch (status)
        {
            case VitalStatus.Normal:
                normal++;
                break;
            case VitalStatus.Borderline:
                borderline++;
                borderlineList.Add($"{label}: {value}{(string.IsNullOrEmpty(unit) ? "" : " " + unit)}");
                break;
            case VitalStatus.Abnormal:
                abnormal++;
                abnormalList.Add($"{label}: {value}{(string.IsNullOrEmpty(unit) ? "" : " " + unit)}");
                break;
        }
    }

    private static string ResolveIconKey(string? serviceName)
    {
        var s = (serviceName ?? string.Empty).ToLowerInvariant();
        if (s.Contains("triage") || s.Contains("vital")) return "heart";
        if (s.Contains("vision") || s.Contains("eye") || s.Contains("optical")) return "eye";
        if (s.Contains("dental") || s.Contains("tooth")) return "tooth";
        if (s.Contains("mental") || s.Contains("psych")) return "brain";
        if (s.Contains("nutrition") || s.Contains("diet")) return "apple";
        return "default";
    }

    private static List<RiskBarDto> BuildRiskBars(
        decimal? bmi, decimal? systolic, decimal? glucose, decimal? cholesterol)
    {
        var bars = new List<RiskBarDto>();

        if (bmi.HasValue)
            bars.Add(new RiskBarDto
            {
                Name = "Body Mass Index",
                Level = RiskBandMapper.MapBmi(bmi.Value).ToString(),
                Value = bmi.Value.ToString("0.0")
            });

        if (systolic.HasValue)
            bars.Add(new RiskBarDto
            {
                Name = "Blood Pressure",
                Level = RiskBandMapper.MapSystolicBp(systolic.Value).ToString(),
                Value = $"{systolic.Value:0} mmHg"
            });

        if (glucose.HasValue)
            bars.Add(new RiskBarDto
            {
                Name = "Blood Glucose",
                Level = RiskBandMapper.MapBloodGlucose(glucose.Value).ToString(),
                Value = $"{glucose.Value:0.0} mmol/L"
            });

        if (cholesterol.HasValue)
            bars.Add(new RiskBarDto
            {
                Name = "Cholesterol",
                Level = RiskBandMapper.MapCholesterol(cholesterol.Value).ToString(),
                Value = $"{cholesterol.Value:0.0} mmol/L"
            });

        return bars;
    }
}
