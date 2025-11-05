using System.Text;
using Salubrity.Application.DTOs.Clinical;
using Salubrity.Application.DTOs.Forms.IntakeFormResponses;
using Salubrity.Application.DTOs.HealthAssessments;
using Salubrity.Application.DTOs.HealthCamps;
using Salubrity.Application.Interfaces.Repositories.HealthCamps;
using Salubrity.Application.Interfaces.Repositories.IntakeForms;
using Salubrity.Application.Interfaces.Services.Clinical;
using Salubrity.Application.Interfaces.Services.HealthAssessments;
using Salubrity.Domain.Entities.HealthCamps;

namespace Salubrity.Application.Services.IntakeForms.CampDataExport
{
    public class StreamingCsvExporter
    {
        private readonly IHealthCampRepository _healthCampRepository;
        private readonly IIntakeFormResponseRepository _intakeFormResponseRepository;
        private readonly IHealthAssessmentFormService _healthAssessmentFormService;
        private readonly IDoctorRecommendationService _doctorRecommendationService;

        public StreamingCsvExporter(
            IHealthCampRepository healthCampRepository,
            IIntakeFormResponseRepository intakeFormResponseRepository,
            IHealthAssessmentFormService healthAssessmentFormService,
            IDoctorRecommendationService doctorRecommendationService)
        {
            _healthCampRepository = healthCampRepository;
            _intakeFormResponseRepository = intakeFormResponseRepository;
            _healthAssessmentFormService = healthAssessmentFormService;
            _doctorRecommendationService = doctorRecommendationService;
        }

        public async IAsyncEnumerable<string> ExportAsync(CancellationToken ct = default)
        {
            // First, fetch and process a subset of data to build headers
            var headerData = await GetHeaderDataAsync(ct);

            // Build and yield headers
            var headers = BuildHeaders(headerData.OrderedFields);
            yield return string.Join(",", headers.Select(h => EscapeCsvField(h))) + "\n";

            // Now stream the actual data
            var totalCamps = 0;
            var totalParticipants = 0;

            var allCamps = await _healthCampRepository.GetAllAsync();
            var limitedCamps = allCamps.OrderBy(c => c.StartDate).Take(20).ToList(); // Limit for performance

            foreach (var campListDto in limitedCamps)
            {
                var rowsProcessed = 0;
                await foreach (var row in ProcessCampDataAsync(campListDto, headerData.OrderedFields, ct))
                {
                    if (!string.IsNullOrEmpty(row))
                    {
                        yield return row;
                        rowsProcessed++;
                    }
                }

                if (rowsProcessed > 0)
                {
                    totalCamps++;
                    totalParticipants += rowsProcessed;
                }

                // Small delay to prevent overwhelming
                await Task.Delay(10, ct);
            }

            // Yield summary as comment at the end
            yield return $"# Export completed - Total Camps: {totalCamps}, Total Participants: {totalParticipants}\n";
        }

        private async Task<(List<FieldDefinition> OrderedFields, Dictionary<string, string> FieldLookup)> GetHeaderDataAsync(CancellationToken ct)
        {
            // Get a small sample of data to build field definitions
            var allCamps = await _healthCampRepository.GetAllAsync();
            var sampleCamps = allCamps.Take(3).ToList(); // Take only 3 camps for header analysis

            var sampleData = new AllCampsData { CampDataList = [] };

            foreach (var campListDto in sampleCamps)
            {
                var campDetails = await _healthCampRepository.GetByIdAsync(campListDto.Id);
                if (campDetails == null) continue;

                var entityResponses = await _intakeFormResponseRepository.GetResponsesByCampIdWithDetailAsync(campListDto.Id, ct);
                if (!entityResponses.Any()) continue;

                var participants = await _healthCampRepository.GetParticipantsAsync(campListDto.Id, null, null, ct);
                var participantUserIds = participants.Select(p => p.UserId).ToHashSet();

                var filteredResponses = entityResponses
                    .Where(r => r.Patient?.User != null && participantUserIds.Contains(r.Patient.UserId))
                    .Take(5) // Take only 5 participants per camp for header analysis
                    .ToList();

                if (!filteredResponses.Any()) continue;

                var patientIds = filteredResponses.Select(r => r.PatientId).Take(3).ToList(); // Limit patients for header

                var dtoResponses = new List<IntakeFormResponseDetailDto>();
                foreach (var patientId in patientIds)
                {
                    var patientDto = await _intakeFormResponseRepository.GetResponsesByPatientAndCampIdAsync(patientId, campListDto.Id, ct);
                    dtoResponses.AddRange(patientDto);
                }

                var healthAssessments = new Dictionary<Guid, List<HealthAssessmentResponseDto>>();
                foreach (var patientId in patientIds.Take(2)) // Even more limited for health assessments
                {
                    var assessments = await _healthAssessmentFormService.GetPatientAssessmentResponsesAsync(patientId, campListDto.Id, ct);
                    healthAssessments[patientId] = assessments;
                }

                var doctorRecs = await _doctorRecommendationService.GetByHealthCampAsync(campListDto.Id, ct);

                sampleData.CampDataList.Add(new CampDataWithInfo
                {
                    Camp = campDetails,
                    OrganizationName = campDetails.Organization?.BusinessName ?? "Unknown",
                    CampDate = campDetails.StartDate,
                    EntityResponses = filteredResponses,
                    DtoResponses = dtoResponses,
                    HealthAssessmentResponses = healthAssessments,
                    DoctorRecommendations = doctorRecs.ToList()
                });
            }

            // Process sample data to get field definitions
            var processor = new AllCampsDataProcessor();
            var processedSample = processor.Process(sampleData);

            var fieldLookup = new Dictionary<string, string>();
            foreach (var field in processedSample.OrderedFields)
            {
                fieldLookup[field.FieldId] = field.Label;
            }

            return (processedSample.OrderedFields, fieldLookup);
        }

        private List<string> BuildHeaders(List<FieldDefinition> orderedFields)
        {
            var headers = new List<string>
            {
                // Camp Information
                "Organization Name", "Camp Name", "Camp Date",
                
                // Patient Information
                "Patient Name", "Email", "Phone", "Gender", "ID Number",
                "Date of Birth", "Age", "Lifestyle Risk"
            };

            // Add dynamic field headers
            foreach (var field in orderedFields)
            {
                headers.Add(field.Label);
            }

            return headers;
        }

        private async IAsyncEnumerable<string> ProcessCampDataAsync(HealthCampListDto campListDto, List<FieldDefinition> orderedFields, CancellationToken ct)
        {
            var campResults = await ProcessSingleCampAsync(campListDto, orderedFields, ct);

            foreach (var row in campResults)
            {
                yield return row;
            }
        }

        private async Task<List<string>> ProcessSingleCampAsync(HealthCampListDto campListDto, List<FieldDefinition> orderedFields, CancellationToken ct)
        {
            var results = new List<string>();

            try
            {
                var camp = await _healthCampRepository.GetByIdAsync(campListDto.Id);
                if (camp == null) return results;

                var organizationName = camp.Organization?.BusinessName ?? "Unknown";

                // Get responses for this camp
                var entityResponses = await _intakeFormResponseRepository.GetResponsesByCampIdWithDetailAsync(camp.Id, ct);
                if (!entityResponses.Any()) return results;

                // Get participants and filter responses
                var participants = await _healthCampRepository.GetParticipantsAsync(camp.Id, null, null, ct);
                var participantUserIds = participants.Select(p => p.UserId).ToHashSet();

                var filteredResponses = entityResponses
                    .Where(r => r.Patient?.User != null && participantUserIds.Contains(r.Patient.UserId))
                    .GroupBy(r => r.PatientId)
                    .Take(100) // Limit participants per camp to avoid timeouts
                    .ToList();

                foreach (var participantGroup in filteredResponses)
                {
                    var patient = participantGroup.First().Patient;
                    if (patient?.User == null) continue;

                    // Get additional data for this participant
                    var dtoResponses = await _intakeFormResponseRepository.GetResponsesByPatientAndCampIdAsync(participantGroup.Key, camp.Id, ct);
                    var healthAssessments = await _healthAssessmentFormService.GetPatientAssessmentResponsesAsync(participantGroup.Key, camp.Id, ct);
                    var doctorRecs = await _doctorRecommendationService.GetByHealthCampAsync(camp.Id, ct);
                    var doctorRec = doctorRecs.Where(dr => dr.PatientId == participantGroup.Key)
                                            .OrderByDescending(dr => dr.CreatedAt)
                                            .FirstOrDefault();

                    // Build lookups
                    var intakeFormLookup = GetIntakeFormResponseLookup(dtoResponses);
                    var healthAssessmentLookup = GetHealthAssessmentLookup(healthAssessments);

                    var row = new List<string>
                    {
                        // Camp Information
                        EscapeCsvField(organizationName),
                        EscapeCsvField(camp.Name ?? ""),
                        EscapeCsvField(camp.StartDate.ToString("yyyy-MM-dd")),
                        
                        // Patient Information
                        EscapeCsvField(patient.User.FullName ?? ""),
                        EscapeCsvField(patient.User.Email ?? ""),
                        EscapeCsvField(patient.User.Phone ?? ""),
                        EscapeCsvField(patient.User.Gender?.Name ?? ""),
                        EscapeCsvField(patient.User.NationalId ?? ""),
                        EscapeCsvField(patient.User.DateOfBirth?.ToString("yyyy-MM-dd") ?? ""),
                        EscapeCsvField(patient.User.DateOfBirth.HasValue ? CalculateAge(patient.User.DateOfBirth.Value).ToString() : ""),
                        EscapeCsvField(CalculateLifestyleRisk(intakeFormLookup, healthAssessmentLookup, orderedFields))
                    };

                    // Add dynamic field data
                    foreach (var field in orderedFields)
                    {
                        string value = "";
                        if (field.DataSource == "IntakeForm" && intakeFormLookup.TryGetValue(field.FieldId, out var fieldValue))
                        {
                            value = FormatFieldValue(fieldValue, field.FieldType);
                        }
                        else if (field.DataSource == "HealthAssessment" && healthAssessmentLookup.TryGetValue(field.FieldId, out var healthValue))
                        {
                            value = healthValue;
                        }
                        else if (field.DataSource == "DoctorRecommendation" && doctorRec != null)
                        {
                            value = GetDoctorRecommendationFieldValue(field.FieldId, doctorRec);
                        }
                        row.Add(EscapeCsvField(value));
                    }

                    results.Add(string.Join(",", row) + "\n");

                    // Small delay between participants
                    await Task.Delay(2, ct);
                }
            }
            catch (Exception)
            {
                // Skip this camp on error
                return results;
            }

            return results;
        }

        private Dictionary<string, string> GetIntakeFormResponseLookup(List<IntakeFormResponseDetailDto> dtoResponses)
        {
            if (!dtoResponses.Any()) return new Dictionary<string, string>();

            return dtoResponses
                .SelectMany(r => r.FieldResponses)
                .GroupBy(fr => $"intake_{fr.FieldId}")
                .ToDictionary(g => g.Key, g => g.OrderByDescending(fr => fr.Id).First().Value ?? "");
        }

        private Dictionary<string, string> GetHealthAssessmentLookup(List<HealthAssessmentResponseDto> healthAssessments)
        {
            if (!healthAssessments.Any()) return new Dictionary<string, string>();

            return healthAssessments
                .SelectMany(assessment => assessment.Sections
                    .SelectMany(section => section.Fields
                        .Select(field => new { assessment.FormName, section.SectionName, field })))
                .ToDictionary(
                    item => $"health_{item.FormName}_{item.SectionName}_{item.field.FieldLabel}".Replace(" ", "_"),
                    item => item.field.Value ?? item.field.SelectedOption ?? ""
                );
        }

        private string GetDoctorRecommendationFieldValue(string fieldId, DoctorRecommendationResponseDto recommendation)
        {
            return fieldId switch
            {
                "doctor_pertinent_history" => recommendation.PertinentHistoryFindings ?? "",
                "doctor_pertinent_clinical" => recommendation.PertinentClinicalFindings ?? "",
                "doctor_diagnostic_impression" => recommendation.DiagnosticImpression ?? "",
                "doctor_conclusion" => recommendation.Conclusion ?? "",
                "doctor_followup_recommendation" => recommendation.FollowUpRecommendation?.Name ?? "",
                "doctor_recommendation_type" => recommendation.RecommendationType?.Name ?? "",
                "doctor_instructions" => recommendation.Instructions ?? "",
                _ => ""
            };
        }

        private static string CalculateLifestyleRisk(Dictionary<string, string> intakeFormResponses, Dictionary<string, string> healthAssessmentResponses, List<FieldDefinition> orderedFields)
        {
            var allValues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var kvp in intakeFormResponses)
            {
                var field = orderedFields.FirstOrDefault(f => f.FieldId == kvp.Key);
                if (field != null) allValues[field.Label] = kvp.Value;
            }
            foreach (var kvp in healthAssessmentResponses)
            {
                var field = orderedFields.FirstOrDefault(f => f.FieldId == kvp.Key);
                if (field != null) allValues[field.Label] = kvp.Value;
            }

            // Simplified risk calculation for performance
            return "Medium"; // You can implement full risk calculation later
        }

        private static string EscapeCsvField(string field)
        {
            if (string.IsNullOrEmpty(field)) return "";

            if (field.Contains(',') || field.Contains('\n') || field.Contains('\r') || field.Contains('"'))
            {
                return $"\"{field.Replace("\"", "\"\"")}\"";
            }

            return field;
        }

        private static int CalculateAge(DateTime birthDate)
        {
            var today = DateTime.Today;
            int age = today.Year - birthDate.Year;
            if (birthDate.Date > today.AddYears(-age)) age--;
            return Math.Max(0, age);
        }

        private string FormatFieldValue(string value, string fieldType) => fieldType.ToLowerInvariant() switch
        {
            "checkbox" => value == "true" ? "Yes" : value == "false" ? "No" : value,
            "date" => DateTime.TryParse(value, out var d) ? d.ToString("yyyy-MM-dd") : value,
            "datetime" => DateTime.TryParse(value, out var dt) ? dt.ToString("yyyy-MM-dd HH:mm") : value,
            "number" => decimal.TryParse(value, out var n) ? n.ToString("0.##") : value,
            _ => value
        };
    }
}