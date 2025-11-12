using ClosedXML.Excel;
using Microsoft.Extensions.Logging;
using Salubrity.Application.DTOs.Clinical;
using Salubrity.Application.DTOs.Forms.IntakeFormResponses;
using System.Diagnostics;

namespace Salubrity.Application.Services.IntakeForms.CampDataExport
{
    public class MultiCampDataExcelExporter
    {
        private readonly ILogger<MultiCampDataExcelExporter>? _logger;

        public MultiCampDataExcelExporter(ILogger<MultiCampDataExcelExporter>? logger = null)
        {
            _logger = logger;
        }

        public byte[] Export(ProcessedMultiCampData data)
        {
            var overallStopwatch = Stopwatch.StartNew();
            _logger?.LogInformation("====== STARTING EXCEL EXPORT ======");

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("All Camps Data Export");

            var sw = Stopwatch.StartNew();
            var headerStructure = CreateHeaderStructure(data.FieldsBySection);
            sw.Stop();
            _logger?.LogInformation("⏱️ Create header structure: {ElapsedMs}ms", sw.ElapsedMilliseconds);

            sw = Stopwatch.StartNew();
            var totalColumns = SetupStructuredHeaders(worksheet, headerStructure);
            sw.Stop();
            _logger?.LogInformation("⏱️ Setup structured headers: {ElapsedMs}ms ({Columns} columns)",
                sw.ElapsedMilliseconds, totalColumns);

            sw = Stopwatch.StartNew();
            PopulateDataRows(worksheet, data);
            sw.Stop();
            _logger?.LogInformation("⏱️ Populate data rows: {ElapsedMs}ms ({Rows} rows)",
                sw.ElapsedMilliseconds, data.ParticipantRows.Count);

            sw = Stopwatch.StartNew();
            ApplyWorksheetStyling(worksheet, totalColumns, 3 + data.ParticipantRows.Count);
            sw.Stop();
            _logger?.LogInformation("⏱️ Apply worksheet styling: {ElapsedMs}ms", sw.ElapsedMilliseconds);

            sw = Stopwatch.StartNew();
            AddSummarySection(worksheet, data, 3 + data.ParticipantRows.Count + 2);
            sw.Stop();
            _logger?.LogInformation("⏱️ Add summary section: {ElapsedMs}ms", sw.ElapsedMilliseconds);

            worksheet.SheetView.Freeze(3, 7);

            sw = Stopwatch.StartNew();
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var excelData = stream.ToArray();
            sw.Stop();
            _logger?.LogInformation("⏱️ Save to stream: {ElapsedMs}ms ({FileSize} bytes)",
                sw.ElapsedMilliseconds, excelData.Length);

            overallStopwatch.Stop();
            _logger?.LogInformation("====== EXCEL EXPORT SUMMARY ======");
            _logger?.LogInformation("⏱️ Total export time: {TotalSeconds:F2}s ({TotalMs}ms)",
                overallStopwatch.Elapsed.TotalSeconds, overallStopwatch.ElapsedMilliseconds);

            return excelData;
        }

        private void PopulateDataRows(IXLWorksheet worksheet, ProcessedMultiCampData data)
        {
            int currentRow = 4;
            foreach (var participantRow in data.ParticipantRows)
            {
                var participant = participantRow.ParticipantGroup.First().Patient;
                if (participant?.User == null) continue;

                // Camp Information (first 3 columns)
                worksheet.Cell(currentRow, 1).Value = participantRow.OrganizationName;
                worksheet.Cell(currentRow, 2).Value = participantRow.CampName;
                worksheet.Cell(currentRow, 3).Value = participantRow.CampDate.ToString("yyyy-MM-dd");

                // Patient Information
                worksheet.Cell(currentRow, 4).Value = participant.User.FullName ?? "";
                worksheet.Cell(currentRow, 5).Value = participant.User.Email ?? "";
                worksheet.Cell(currentRow, 6).Value = participant.User.Phone ?? "";
                worksheet.Cell(currentRow, 7).Value = participant.User.Gender?.Name ?? "";
                worksheet.Cell(currentRow, 8).Value = participant.User.NationalId ?? "";
                worksheet.Cell(currentRow, 9).Value = participant.User.DateOfBirth?.ToString("yyyy-MM-dd") ?? "";
                worksheet.Cell(currentRow, 10).Value = participant.User.DateOfBirth.HasValue
                    ? CalculateAge(participant.User.DateOfBirth.Value, DateTime.Now).ToString()
                    : "";

                // Get response lookups
                var intakeFormResponseLookup = GetIntakeFormResponseLookup(participantRow.DtoResponses);
                var healthAssessmentResponseLookup = participantRow.HealthAssessmentResponses;
                var doctorRecommendation = participantRow.DoctorRecommendation;

                // Calculate and set lifestyle risk
                var lifestyleRisk = CalculateLifestyleRisk(intakeFormResponseLookup, healthAssessmentResponseLookup, data.OrderedFields);
                worksheet.Cell(currentRow, 11).Value = lifestyleRisk;
                worksheet.Cell(currentRow, 11).Style.Fill.BackgroundColor = GetLifestyleRiskColor(lifestyleRisk);
                worksheet.Cell(currentRow, 11).Style.Font.Bold = true;

                // Fill dynamic field data
                int columnIndex = 12;
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
                    worksheet.Cell(currentRow, columnIndex).Value = value;
                    columnIndex++;
                }

                currentRow++;
            }
        }

        private CampDataExcelExporter.HeaderStructure CreateHeaderStructure(IEnumerable<IGrouping<dynamic, FieldDefinition>> fieldsBySection)
        {
            var structure = new CampDataExcelExporter.HeaderStructure();

            // Add Camp Information section
            structure.AddSection("Camp Information", new List<CampDataExcelExporter.HeaderColumn>
            {
                new("Organization Name", "Org"),
                new("Camp Name", "Camp"),
                new("Camp Date", "Date")
            });

            // Add fixed participant columns
            structure.AddSection("Patient Information", new List<CampDataExcelExporter.HeaderColumn>
            {
                new("Name", "Name"),
                new("Email", "Email"),
                new("Phone", "Phone"),
                new("Gender", "Gender"),
                new("ID Number", "ID"),
                new("Date of Birth", "DOB"),
                new("Age", "Age"),
                new("Lifestyle Risk", "Risk")
            });

            // Add dynamic fields (same as single camp export)
            foreach (var sectionGroup in fieldsBySection)
            {
                var fieldsInSection = sectionGroup.OrderBy(f => GetFieldPriority(f.Label)).ThenBy(f => f.Order).ToList();

                var mainSectionName = GetMainSectionName(sectionGroup.Key.SectionName);
                var subSectionName = GetSubSectionName(sectionGroup.Key.SectionName);

                var columns = fieldsInSection.Select(field => new CampDataExcelExporter.HeaderColumn(
                    field.Label,
                    TruncateFieldName(field.Label),
                    subSectionName
                )).ToList();

                structure.AddSection(mainSectionName, columns);
            }

            return structure;
        }

        private int SetupStructuredHeaders(IXLWorksheet worksheet, CampDataExcelExporter.HeaderStructure headerStructure)
        {
            int currentCol = 1;

            foreach (var section in headerStructure.Sections)
            {
                int sectionStartCol = currentCol;

                foreach (var subSection in section.SubSections)
                {
                    int subSectionStartCol = currentCol;

                    foreach (var column in subSection.Columns)
                    {
                        worksheet.Cell(3, currentCol).Value = column.ShortName;
                        worksheet.Cell(3, currentCol).Style.Font.Bold = true;
                        worksheet.Cell(3, currentCol).Style.Alignment.WrapText = true;
                        worksheet.Cell(3, currentCol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                        currentCol++;
                    }

                    if (subSection.Columns.Count > 1 && !string.IsNullOrEmpty(subSection.Name))
                    {
                        worksheet.Range(2, subSectionStartCol, 2, currentCol - 1).Merge();
                        worksheet.Cell(2, subSectionStartCol).Value = subSection.Name;
                        worksheet.Cell(2, subSectionStartCol).Style.Font.Bold = true;
                        worksheet.Cell(2, subSectionStartCol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        worksheet.Cell(2, subSectionStartCol).Style.Fill.BackgroundColor = GetSectionColor(subSection.Name);
                    }
                    else if (subSection.Columns.Count == 1)
                    {
                        worksheet.Cell(2, subSectionStartCol).Value = subSection.Name ?? "";
                        worksheet.Cell(2, subSectionStartCol).Style.Font.Bold = true;
                        worksheet.Cell(2, subSectionStartCol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        worksheet.Cell(2, subSectionStartCol).Style.Fill.BackgroundColor = GetSectionColor(subSection.Name ?? "");
                    }
                }

                if (currentCol - sectionStartCol > 1)
                {
                    worksheet.Range(1, sectionStartCol, 1, currentCol - 1).Merge();
                    worksheet.Cell(1, sectionStartCol).Value = section.Name;
                    worksheet.Cell(1, sectionStartCol).Style.Font.Bold = true;
                    worksheet.Cell(1, sectionStartCol).Style.Font.FontSize = 12;
                    worksheet.Cell(1, sectionStartCol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(1, sectionStartCol).Style.Fill.BackgroundColor = GetMainSectionColor(section.Name);
                }
                else
                {
                    worksheet.Cell(1, sectionStartCol).Value = section.Name;
                    worksheet.Cell(1, sectionStartCol).Style.Font.Bold = true;
                    worksheet.Cell(1, sectionStartCol).Style.Font.FontSize = 12;
                    worksheet.Cell(1, sectionStartCol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(1, sectionStartCol).Style.Fill.BackgroundColor = GetMainSectionColor(section.Name);
                }
            }

            return currentCol - 1;
        }

        private void ApplyWorksheetStyling(IXLWorksheet worksheet, int totalColumns, int totalRows)
        {
            var headerRange = worksheet.Range(1, 1, 3, totalColumns);
            headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thick;
            headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            headerRange.Style.Font.Bold = true;

            if (totalRows > 3)
            {
                var dataRange = worksheet.Range(4, 1, totalRows, totalColumns);
                dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Hair;

                for (int row = 4; row <= totalRows; row++)
                {
                    if ((row - 4) % 2 == 0)
                    {
                        worksheet.Range(row, 1, row, totalColumns).Style.Fill.BackgroundColor = XLColor.AliceBlue;
                    }
                }
            }

            worksheet.Columns().AdjustToContents();
            foreach (var column in worksheet.Columns(1, totalColumns))
            {
                if (column.Width > 40) column.Width = 40;
                if (column.Width < 12) column.Width = 12;
            }

            worksheet.Row(1).Height = 25;
            worksheet.Row(2).Height = 20;
            worksheet.Row(3).Height = 30;
        }

        private void AddSummarySection(IXLWorksheet worksheet, ProcessedMultiCampData data, int startRow)
        {
            worksheet.Cell(startRow, 1).Value = "Export Summary:";
            worksheet.Cell(startRow, 1).Style.Font.Bold = true;
            worksheet.Cell(startRow, 1).Style.Font.FontSize = 12;

            worksheet.Cell(startRow + 1, 1).Value = $"Total Camps: {data.TotalCamps}";
            worksheet.Cell(startRow + 2, 1).Value = $"Total Participants: {data.TotalParticipants}";
            worksheet.Cell(startRow + 3, 1).Value = $"Total Intake Form Fields: {data.IntakeFieldCount}";
            worksheet.Cell(startRow + 4, 1).Value = $"Total Health Assessment Fields: {data.HealthFieldCount}";
            worksheet.Cell(startRow + 5, 1).Value = $"Total Doctor Recommendation Fields: {data.DoctorRecommendationFieldCount}";
            worksheet.Cell(startRow + 6, 1).Value = $"Total Sections: {data.FieldsBySection.Count}";
            worksheet.Cell(startRow + 7, 1).Value = $"Export Date: {DateTime.Now.AddHours(3):yyyy-MM-dd HH:mm:ss}";

            var summaryRange = worksheet.Range(startRow, 1, startRow + 7, 2);
            summaryRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            summaryRange.Style.Fill.BackgroundColor = XLColor.LightYellow;
        }

        #region Helper Methods
        private Dictionary<string, string> GetIntakeFormResponseLookup(List<IntakeFormResponseDetailDto> dtoList)
        {
            if (dtoList.Any())
            {
                return dtoList
                    .SelectMany(r => r.FieldResponses)
                    .GroupBy(fr => $"intake_{fr.FieldId}")
                    .ToDictionary(g => g.Key, g => g.OrderByDescending(fr => fr.Id).First().Value ?? "");
            }
            return new Dictionary<string, string>();
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

        private static int GetFieldPriority(string fieldLabel)
        {
            var label = fieldLabel.ToLowerInvariant();
            if (label.Contains("height")) return 1;
            if (label.Contains("weight")) return 2;
            if (label.Contains("bmi")) return 3;
            if (label.Contains("temperature")) return 4;
            if (label.Contains("heart rate")) return 5;
            if (label.Contains("blood pressure") && !label.Contains("reading")) return 6;
            if (label.Contains("bp reading 2")) return 7;
            if (label.Contains("bp reading 3")) return 8;
            if (label.Contains("oxygen")) return 9;
            if (label.Contains("blood sugar") && !label.Contains("reading")) return 10;
            if (label.Contains("rbs reading 2")) return 11;
            if (label.Contains("rbs reading 3")) return 12;
            if (label.Contains("rs")) return 1;
            if (label.Contains("cvs")) return 2;
            if (label.Contains("abdomen")) return 3;
            if (label.Contains("cns")) return 4;
            if (label.Contains("skin")) return 5;
            if (label.Contains("right eye")) return 1;
            if (label.Contains("left eye")) return 2;
            if (label.Contains("vision")) return 1;
            if (label.Contains("sphere")) return 2;
            if (label.Contains("cylinder")) return 3;
            if (label.Contains("axis")) return 4;
            if (label.Contains("va")) return 5;
            if (label.Contains("add")) return 6;
            if (label.Contains("findings")) return 1;
            if (label.Contains("conclusion")) return 2;
            if (label.Contains("recommendations")) return 3;
            if (label.Contains("additional notes")) return 4;
            if (label.Contains("body mass index")) return 1;
            if (label.Contains("body fat")) return 2;
            if (label.Contains("body water")) return 3;
            if (label.Contains("muscle mass")) return 4;
            if (label.Contains("basal metabolic")) return 5;
            if (label.Contains("bone density")) return 6;
            if (label.Contains("visceral fat")) return 7;
            if (label.Contains("metabolic age")) return 8;
            if (label.Contains("problem")) return 9;
            if (label.Contains("plan")) return 10;
            return 50;
        }

        private static string GetBMIRiskCategory(decimal bmi) => bmi switch { >= 18 and <= 22.9m => "Very Low", >= 23 and <= 24.9m => "Low", >= 25 and <= 27.9m => "Medium", >= 28 and <= 30m => "High", > 30 => "Very High", _ => "No Data" };
        private static string GetBloodPressureRiskCategory(decimal s, decimal d) { if (d == 0) return s switch { < 120 => "Very Low", >= 120 and <= 139 => "Low", >= 140 and <= 150 => "Medium", >= 151 and <= 159 => "High", >= 160 => "Very High", _ => "No Data" }; if (s < 120 && d < 80) return "Very Low"; if (s <= 139 && d <= 89) return "Low"; if (s <= 150 && d <= 95) return "Medium"; if (s <= 159 && d <= 99) return "High"; if (s >= 160 || d >= 100) return "Very High"; return "No Data"; }
        private static string GetBloodGlucoseRiskCategory(decimal g) => g switch { < 7 => "Very Low", >= 7 and <= 10 => "Medium", >= 10 and <= 10.9m => "High", > 11 => "Very High", _ => "No Data" };
        private static string GetCholesterolRiskCategory(decimal c) => c switch { >= 2.3m and <= 4.9m => "Very Low", >= 5 and <= 5.17m => "Low", >= 5.18m and <= 6.19m => "Medium", > 6.2m and <= 7m => "High", > 7 => "Very High", _ => "No Data" };
        private static int ConvertRiskToScore(string risk) => risk switch { "Very Low" => 1, "Low" => 2, "Medium" => 3, "High" => 4, "Very High" => 5, _ => 0 };
        private static string ConvertScoreToRisk(double score) => score switch { <= 1.5 => "Very Low", <= 2.5 => "Low", <= 3.5 => "Medium", <= 4.5 => "High", _ => "Very High" };
        private static XLColor GetLifestyleRiskColor(string risk) => risk switch { "Very Low" => XLColor.LightGreen, "Low" => XLColor.LightYellow, "Medium" => XLColor.Orange, "High" => XLColor.LightCoral, "Very High" => XLColor.Red, _ => XLColor.LightGray };
        private static XLColor GetSectionColor(string sectionName) => sectionName.ToLowerInvariant() switch { var s when s.Contains("personal") || s.Contains("demographic") => XLColor.LightBlue, var s when s.Contains("medical") || s.Contains("health") => XLColor.LightGreen, var s when s.Contains("assessment") || s.Contains("evaluation") => XLColor.LightYellow, var s when s.Contains("vitals") || s.Contains("vital") => XLColor.LightPink, var s when s.Contains("lab") || s.Contains("laboratory") => XLColor.LightCyan, var s when s.Contains("history") || s.Contains("background") => XLColor.Lavender, var s when s.Contains("physical") || s.Contains("examination") => XLColor.LightSalmon, var s when s.Contains("mental") || s.Contains("psychological") => XLColor.LightSteelBlue, var s when s.Contains("nutrition") || s.Contains("dietary") => XLColor.LightGoldenrodYellow, var s when s.Contains("recommendation") || s.Contains("advice") || s.Contains("doctor") => XLColor.LightSeaGreen, "general" => XLColor.WhiteSmoke, _ => XLColor.LightGray };
        private string GetMainSectionName(string fullSectionName) => fullSectionName.Contains("Health Assessment") ? "Health Assessments" : fullSectionName.Contains("Doctor Recommendations") ? "Doctor Recommendations" : fullSectionName.Split('-')[0].Trim();
        private string GetSubSectionName(string fullSectionName) { if (fullSectionName.Contains("Health Assessment")) { var parts = fullSectionName.Split(" - "); return parts.Length >= 3 ? $"{parts[1]} - {parts[2]}" : parts.Length > 1 ? parts[1] : fullSectionName; } return fullSectionName; }
        private string TruncateFieldName(string fieldName) { var replacements = new Dictionary<string, string> { { "Do you have", "Have" }, { "Have you ever", "Ever" }, { "Are you currently", "Currently" }, { "Please specify", "Specify" }, { "How many", "Count" }, { "What is your", "Your" }, { "Date of Birth", "DOB" }, { "Blood Pressure", "BP" }, { "Heart Rate", "HR" }, { "Body Mass Index", "BMI" }, { "Temperature", "Temp" }, { "medication", "meds" }, { "family history", "family hist" }, { "medical history", "med hist" }, { "Pertinent History Findings", "History" }, { "Pertinent Clinical Findings", "Clinical" }, { "Diagnostic Impression", "Diagnosis" }, { "Follow-up Recommendation", "Follow-up" }, { "Recommendation Type", "Type" } }; string shortened = fieldName; foreach (var r in replacements) shortened = shortened.Replace(r.Key, r.Value, StringComparison.OrdinalIgnoreCase); return shortened.Replace("  ", " ").Trim(); }
        private XLColor GetMainSectionColor(string sectionName) => sectionName.ToLowerInvariant() switch { "camp information" => XLColor.LightCyan, "patient information" => XLColor.LightGray, "health assessments" => XLColor.LightBlue, "doctor recommendations" => XLColor.LightSeaGreen, "personal information" => XLColor.LightGreen, "medical history" => XLColor.LightCoral, "lifestyle" => XLColor.LightYellow, "emergency contact" => XLColor.LightPink, _ => XLColor.LightCyan };
        private string FormatFieldValue(string value, string fieldType) => fieldType.ToLowerInvariant() switch { "checkbox" => value == "true" ? "Yes" : value == "false" ? "No" : value, "date" => DateTime.TryParse(value, out var d) ? d.ToString("yyyy-MM-dd") : value, "datetime" => DateTime.TryParse(value, out var dt) ? dt.ToString("yyyy-MM-dd HH:mm") : value, "number" => decimal.TryParse(value, out var n) ? n.ToString("0.##") : value, _ => value };
        #endregion
    }
}