using System.Text;
using Salubrity.Application.DTOs.Clinical;
using Salubrity.Application.DTOs.Forms.IntakeFormResponses;

namespace Salubrity.Application.Services.IntakeForms.CampDataExport
{
    public class AllCampsDataCsvExporter
    {
        public byte[] Export(ProcessedAllCampsData data)
        {
            var csv = new StringBuilder();

            // Create headers
            var headers = new List<string>
            {
                // Camp Information
                "Organization Name",
                "Camp Name",
                "Camp Date",
                
                // Patient Information
                "Patient Name",
                "Email",
                "Phone",
                "Gender",
                "ID Number",
                "Date of Birth",
                "Age",
                "Lifestyle Risk"
            };

            // Add dynamic field headers
            foreach (var field in data.OrderedFields)
            {
                headers.Add(EscapeCsvField(field.Label));
            }

            // Write headers
            csv.AppendLine(string.Join(",", headers));

            // Write data rows
            foreach (var participantData in data.ParticipantResponses)
            {
                var participant = participantData.Patient;
                if (participant?.User == null) continue;

                var row = new List<string>();

                // Camp Information
                row.Add(EscapeCsvField(participantData.OrganizationName));
                row.Add(EscapeCsvField(participantData.CampName));
                row.Add(EscapeCsvField(participantData.CampDate.ToString("yyyy-MM-dd")));

                // Patient Information
                row.Add(EscapeCsvField(participant.User.FullName ?? ""));
                row.Add(EscapeCsvField(participant.User.Email ?? ""));
                row.Add(EscapeCsvField(participant.User.Phone ?? ""));
                row.Add(EscapeCsvField(participant.User.Gender?.Name ?? ""));
                row.Add(EscapeCsvField(participant.User.NationalId ?? ""));
                row.Add(EscapeCsvField(participant.User.DateOfBirth?.ToString("yyyy-MM-dd") ?? ""));
                row.Add(EscapeCsvField(participant.User.DateOfBirth.HasValue ? CalculateAge(participant.User.DateOfBirth.Value, DateTime.Now).ToString() : ""));

                // Get response lookups
                var intakeFormResponseLookup = GetIntakeFormResponseLookup(data.DtoResponseLookup, participant.Id);
                var healthAssessmentResponseLookup = data.HealthAssessmentLookup.TryGetValue(participant.Id, out var healthResponses)
                    ? healthResponses
                    : new Dictionary<string, string>();

                // Get doctor recommendation for this patient
                var doctorRecommendation = data.DoctorRecommendationLookup.TryGetValue(participant.Id, out var recommendation)
                    ? recommendation
                    : null;

                // Calculate and add lifestyle risk
                var lifestyleRisk = CalculateLifestyleRisk(intakeFormResponseLookup, healthAssessmentResponseLookup, data.OrderedFields);
                row.Add(EscapeCsvField(lifestyleRisk));

                // Add dynamic field data
                foreach (var field in data.OrderedFields)
                {
                    string value = "";
                    if (field.DataSource == "IntakeForm" && intakeFormResponseLookup.TryGetValue(field.FieldId, out var fieldValue))
                    {
                        value = FormatFieldValue(fieldValue, field.FieldType);
                    }
                    else if (field.DataSource == "HealthAssessment" && healthAssessmentResponseLookup.TryGetValue(field.FieldId, out var healthValue))
                    {
                        value = healthValue;
                    }
                    else if (field.DataSource == "DoctorRecommendation" && doctorRecommendation != null)
                    {
                        value = GetDoctorRecommendationFieldValue(field.FieldId, doctorRecommendation);
                    }
                    row.Add(EscapeCsvField(value));
                }

                csv.AppendLine(string.Join(",", row));
            }

            return Encoding.UTF8.GetBytes(csv.ToString());
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
                "doctor_created_date" => recommendation.CreatedAt.ToString("yyyy-MM-dd"),
                _ => ""
            };
        }

        private Dictionary<string, string> GetIntakeFormResponseLookup(Dictionary<Guid, List<IntakeFormResponseDetailDto>> dtoLookup, Guid patientId)
        {
            if (dtoLookup.TryGetValue(patientId, out var dtoList) && dtoList.Any())
            {
                return dtoList
                    .SelectMany(r => r.FieldResponses)
                    .GroupBy(fr => $"intake_{fr.FieldId}")
                    .ToDictionary(g => g.Key, g => g.OrderByDescending(fr => fr.Id).First().Value ?? "");
            }

            return new Dictionary<string, string>();
        }

        private static string EscapeCsvField(string field)
        {
            if (string.IsNullOrEmpty(field)) return "";

            // If field contains comma, newline, or quote, wrap in quotes and escape internal quotes
            if (field.Contains(',') || field.Contains('\n') || field.Contains('\r') || field.Contains('"'))
            {
                return $"\"{field.Replace("\"", "\"\"")}\"";
            }

            return field;
        }

        private static int CalculateAge(DateTime birthDate, DateTime currentDate)
        {
            int age = currentDate.Year - birthDate.Year;
            if (currentDate.Month < birthDate.Month || (currentDate.Month == birthDate.Month && currentDate.Day < birthDate.Day))
            {
                age--;
            }
            return Math.Max(0, age);
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
        private string FormatFieldValue(string value, string fieldType) => fieldType.ToLowerInvariant() switch { "checkbox" => value == "true" ? "Yes" : value == "false" ? "No" : value, "date" => DateTime.TryParse(value, out var d) ? d.ToString("yyyy-MM-dd") : value, "datetime" => DateTime.TryParse(value, out var dt) ? dt.ToString("yyyy-MM-dd HH:mm") : value, "number" => decimal.TryParse(value, out var n) ? n.ToString("0.##") : value, _ => value };
    }
}