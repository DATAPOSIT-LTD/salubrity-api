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
            // FIXED: Sort from newest to oldest (descending order) and process ALL camps
            var sortedCamps = allCamps.OrderByDescending(c => c.StartDate).ToList(); // Process ALL camps

            foreach (var campListDto in sortedCamps)
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
                await Task.Delay(5, ct); // Reduced delay
            }

            // Yield summary as comment at the end
            yield return $"# Export completed - Total Camps: {totalCamps}, Total Participants: {totalParticipants}\n";
        }

        private async Task<(List<FieldDefinition> OrderedFields, Dictionary<string, string> FieldLookup)> GetHeaderDataAsync(CancellationToken ct)
        {
            // FIXED: Get a better sample of data to build field definitions - analyze more camps and participants
            var allCamps = await _healthCampRepository.GetAllAsync();
            var sampleCamps = allCamps.Take(10).ToList(); // Increased from 3 to 10 camps for better header analysis

            var sampleData = new AllCampsData { CampDataList = [] };

            foreach (var campListDto in sampleCamps)
            {
                var campDetails = await _healthCampRepository.GetByIdAsync(campListDto.Id);
                if (campDetails == null) continue;

                var organizationName = await GetOrganizationNameAsync(campDetails, ct);

                var entityResponses = await _intakeFormResponseRepository.GetResponsesByCampIdWithDetailAsync(campListDto.Id, ct);
                if (!entityResponses.Any()) continue;

                var participants = await _healthCampRepository.GetParticipantsAsync(campListDto.Id, null, null, ct);
                var participantUserIds = participants.Select(p => p.UserId).ToHashSet();

                var filteredResponses = entityResponses
                    .Where(r => r.Patient?.User != null && participantUserIds.Contains(r.Patient.UserId))
                    .Take(20) // Increased from 5 to 20 participants for better header analysis
                    .ToList();

                if (!filteredResponses.Any()) continue;

                var patientIds = filteredResponses.Select(r => r.PatientId).Take(10).ToList(); // Increased from 3 to 10

                var dtoResponses = new List<IntakeFormResponseDetailDto>();
                foreach (var patientId in patientIds)
                {
                    var patientDto = await _intakeFormResponseRepository.GetResponsesByPatientAndCampIdAsync(patientId, campListDto.Id, ct);
                    dtoResponses.AddRange(patientDto);
                }

                var healthAssessments = new Dictionary<Guid, List<HealthAssessmentResponseDto>>();
                foreach (var patientId in patientIds.Take(5)) // Increased from 2 to 5
                {
                    var assessments = await _healthAssessmentFormService.GetPatientAssessmentResponsesAsync(patientId, campListDto.Id, ct);
                    healthAssessments[patientId] = assessments;
                }

                var doctorRecs = await _doctorRecommendationService.GetByHealthCampAsync(campListDto.Id, ct);

                sampleData.CampDataList.Add(new CampDataWithInfo
                {
                    Camp = campDetails,
                    OrganizationName = organizationName,
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

                var organizationName = await GetOrganizationNameAsync(camp, ct);

                // Get responses for this camp
                var entityResponses = await _intakeFormResponseRepository.GetResponsesByCampIdWithDetailAsync(camp.Id, ct);
                if (!entityResponses.Any()) return results;

                // Get participants and filter responses
                var participants = await _healthCampRepository.GetParticipantsAsync(camp.Id, null, null, ct);
                var participantUserIds = participants.Select(p => p.UserId).ToHashSet();

                var filteredResponses = entityResponses
                    .Where(r => r.Patient?.User != null && participantUserIds.Contains(r.Patient.UserId))
                    .GroupBy(r => r.PatientId)
                    // FIXED: Remove participant limit to get ALL participants
                    .ToList(); // No more .Take(100) limit

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

                    // FIXED: Remove or reduce delays to improve performance
                    // await Task.Delay(2, ct); // Removed delay between participants
                }
            }
            catch (Exception)
            {
                // Skip this camp on error
                return results;
            }

            return results;
        }

        private async Task<string> GetOrganizationNameAsync(HealthCamp camp, CancellationToken ct)
        {
            // First try: Direct organization relationship
            if (camp.Organization?.BusinessName != null)
            {
                return camp.Organization.BusinessName;
            }

            // Second try: Get camp details which might have ClientName
            try
            {
                var campDetails = await _healthCampRepository.GetCampDetailsByIdAsync(camp.Id);
                if (campDetails?.ClientName != null)
                {
                    return campDetails.ClientName;
                }
            }
            catch
            {
                // Fallback if details fail
            }

            // Third try: Get organization from participants
            try
            {
                var participants = await _healthCampRepository.GetParticipantsAsync(camp.Id, null, null, ct);
                var orgNameFromParticipants = participants.FirstOrDefault()?.HealthCamp?.Organization?.BusinessName;
                if (!string.IsNullOrEmpty(orgNameFromParticipants))
                {
                    return orgNameFromParticipants;
                }
            }
            catch
            {
                // Fallback if participants fail
            }

            return "Unknown_Organization";
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

            // Use the same risk calculation as the Excel exporter
            var validScores = new List<int>
            {
                ConvertRiskToScore(CalculateBMIRisk(allValues)),
                ConvertRiskToScore(CalculateBloodPressureRisk(allValues)),
                ConvertRiskToScore(CalculateBloodGlucoseRisk(allValues)),
                ConvertRiskToScore(CalculateCholesterolRisk(allValues))
            }
            .Where(score => score > 0).ToList();

            if (!validScores.Any()) return "No Data";
            return ConvertScoreToRisk(validScores.Average());
        }

        #region Risk Calculation Methods (copied from Excel exporter)
        private static string CalculateBMIRisk(Dictionary<string, string> values)
        {
            if (decimal.TryParse(FindValueByKeywords(values, ["bmi", "body mass index"]), out var bmi))
            {
                return GetBMIRiskCategory(bmi);
            }
            if (decimal.TryParse(FindValueByKeywords(values, ["height"]), out var height) && decimal.TryParse(FindValueByKeywords(values, ["weight"]), out var weight) && height > 0)
            {
                var heightInMeters = height > 10 ? height / 100 : height;
                return GetBMIRiskCategory(weight / (heightInMeters * heightInMeters));
            }
            return "No Data";
        }

        private static string CalculateBloodPressureRisk(Dictionary<string, string> values)
        {
            var bpValue = FindValueByKeywords(values, ["blood pressure", "bp reading", "systolic", "diastolic"]);
            if (string.IsNullOrEmpty(bpValue)) return "No Data";

            if (bpValue.Contains('/'))
            {
                var parts = bpValue.Split('/');
                if (parts.Length == 2 && decimal.TryParse(parts[0].Trim(), out var s) && decimal.TryParse(parts[1].Trim(), out var d))
                {
                    return GetBloodPressureRiskCategory(s, d);
                }
            }
            else if (decimal.TryParse(bpValue, out var s))
            {
                return GetBloodPressureRiskCategory(s, 0);
            }
            return "No Data";
        }

        private static string CalculateBloodGlucoseRisk(Dictionary<string, string> values)
        {
            if (decimal.TryParse(FindValueByKeywords(values, ["blood sugar", "glucose", "rbs"]), out var glucose))
            {
                return GetBloodGlucoseRiskCategory(glucose);
            }
            return "No Data";
        }

        private static string CalculateCholesterolRisk(Dictionary<string, string> values)
        {
            if (decimal.TryParse(FindValueByKeywords(values, ["cholesterol", "total cholesterol"]), out var chol))
            {
                return GetCholesterolRiskCategory(chol);
            }
            return "No Data";
        }

        private static string FindValueByKeywords(Dictionary<string, string> values, string[] keywords)
        {
            foreach (var keyword in keywords)
            {
                var key = values.Keys.FirstOrDefault(k => k.Contains(keyword, StringComparison.OrdinalIgnoreCase));
                if (key != null && !string.IsNullOrEmpty(values[key])) return values[key];
            }
            return "";
        }

        private static string GetBMIRiskCategory(decimal bmi) => bmi switch { >= 18 and <= 22.9m => "Very Low", >= 23 and <= 24.9m => "Low", >= 25 and <= 27.9m => "Medium", >= 28 and <= 30m => "High", > 30 => "Very High", _ => "No Data" };
        private static string GetBloodPressureRiskCategory(decimal s, decimal d)
        {
            if (d == 0) return s switch { < 120 => "Very Low", >= 120 and <= 139 => "Low", >= 140 and <= 150 => "Medium", >= 151 and <= 159 => "High", >= 160 => "Very High", _ => "No Data" };
            if (s < 120 && d < 80) return "Very Low"; if (s <= 139 && d <= 89) return "Low"; if (s <= 150 && d <= 95) return "Medium"; if (s <= 159 && d <= 99) return "High"; if (s >= 160 || d >= 100) return "Very High";
            return "No Data";
        }
        private static string GetBloodGlucoseRiskCategory(decimal g) => g switch { < 7 => "Very Low", >= 7 and <= 10 => "Medium", >= 10 and <= 10.9m => "High", > 11 => "Very High", _ => "No Data" };
        private static string GetCholesterolRiskCategory(decimal c) => c switch { >= 2.3m and <= 4.9m => "Very Low", >= 5 and <= 5.17m => "Low", >= 5.18m and <= 6.19m => "Medium", > 6.2m and <= 7m => "High", > 7 => "Very High", _ => "No Data" };
        private static int ConvertRiskToScore(string risk) => risk switch { "Very Low" => 1, "Low" => 2, "Medium" => 3, "High" => 4, "Very High" => 5, _ => 0 };
        private static string ConvertScoreToRisk(double score) => score switch { <= 1.5 => "Very Low", <= 2.5 => "Low", <= 3.5 => "Medium", <= 4.5 => "High", _ => "Very High" };
        #endregion

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