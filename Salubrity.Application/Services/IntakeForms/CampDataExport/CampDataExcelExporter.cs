using ClosedXML.Excel;
using Salubrity.Application.DTOs.Forms.IntakeFormResponses;
using Salubrity.Domain.Entities.IntakeForms;

namespace Salubrity.Application.Services.IntakeForms.CampDataExport
{
    public class CampDataExcelExporter
    {
        public byte[] Export(ProcessedCampData data)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Camp Data Export");

            var headerStructure = CreateHeaderStructure(data.FieldsBySection);
            var totalColumns = SetupStructuredHeaders(worksheet, headerStructure);

            PopulateDataRows(worksheet, data);

            ApplyWorksheetStyling(worksheet, totalColumns, 3 + data.ParticipantResponses.Count);

            AddSummarySection(worksheet, data, 3 + data.ParticipantResponses.Count + 2);
            AddLegendSection(worksheet, data, 3 + data.ParticipantResponses.Count + 11);

            worksheet.SheetView.Freeze(3, 8);

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        private void PopulateDataRows(IXLWorksheet worksheet, ProcessedCampData data)
        {
            int currentRow = 4;
            foreach (var participantGroup in data.ParticipantResponses)
            {
                var participant = participantGroup.First().Patient;
                if (participant?.User == null) continue;

                // Fill basic info
                worksheet.Cell(currentRow, 1).Value = participant.User.FullName ?? "";
                worksheet.Cell(currentRow, 2).Value = participant.User.Email ?? "";
                worksheet.Cell(currentRow, 3).Value = participant.User.Phone ?? "";
                worksheet.Cell(currentRow, 4).Value = participant.User.Gender?.Name ?? "";
                worksheet.Cell(currentRow, 5).Value = participant.User.NationalId ?? "";
                worksheet.Cell(currentRow, 6).Value = participant.User.DateOfBirth?.ToString("yyyy-MM-dd") ?? "";
                worksheet.Cell(currentRow, 7).Value = participant.User.DateOfBirth.HasValue ? CalculateAge(participant.User.DateOfBirth.Value, DateTime.Now).ToString() : "";

                // Get response lookups
                var intakeFormResponseLookup = GetIntakeFormResponseLookup(data.DtoResponseLookup, participantGroup, participant.Id);
                var healthAssessmentResponseLookup = data.HealthAssessmentLookup.TryGetValue(participant.Id, out var healthResponses)
                    ? healthResponses
                    : new Dictionary<string, string>();

                // Calculate and set lifestyle risk
                var lifestyleRisk = CalculateLifestyleRisk(intakeFormResponseLookup, healthAssessmentResponseLookup, data.OrderedFields);
                worksheet.Cell(currentRow, 8).Value = lifestyleRisk;
                worksheet.Cell(currentRow, 8).Style.Fill.BackgroundColor = GetLifestyleRiskColor(lifestyleRisk);
                worksheet.Cell(currentRow, 8).Style.Font.Bold = true;

                // Fill dynamic field data
                int columnIndex = 9;
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
                    worksheet.Cell(currentRow, columnIndex).Value = value;
                    columnIndex++;
                }

                currentRow++;
            }
        }

        private Dictionary<string, string> GetIntakeFormResponseLookup(Dictionary<Guid, List<IntakeFormResponseDetailDto>> dtoLookup, IGrouping<Guid, IntakeFormResponse> participantGroup, Guid patientId)
        {
            if (dtoLookup.TryGetValue(patientId, out var dtoList) && dtoList.Any())
            {
                return dtoList
                    .SelectMany(r => r.FieldResponses)
                    .GroupBy(fr => $"intake_{fr.FieldId}")
                    .ToDictionary(g => g.Key, g => g.OrderByDescending(fr => fr.Id).First().Value ?? "");
            }

            return participantGroup
                .SelectMany(r => r.FieldResponses)
                .GroupBy(fr => $"intake_{fr.FieldId}")
                .ToDictionary(g => g.Key, g => g.OrderByDescending(fr => fr.Id).First().Value ?? "");
        }

        private HeaderStructure CreateHeaderStructure(IEnumerable<IGrouping<dynamic, FieldDefinition>> fieldsBySection)
        {
            var structure = new HeaderStructure();

            // Add fixed participant columns
            structure.AddSection("Patient Information", new List<HeaderColumn>
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

            // Group fields by main section and subsection
            foreach (var sectionGroup in fieldsBySection)
            {
                var fieldsInSection = sectionGroup.OrderBy(f => GetFieldPriority(f.Label)).ThenBy(f => f.Order).ToList();

                var mainSectionName = GetMainSectionName(sectionGroup.Key.SectionName);
                var subSectionName = GetSubSectionName(sectionGroup.Key.SectionName);

                var columns = fieldsInSection.Select(field => new HeaderColumn(
                    field.Label,
                    TruncateFieldName(field.Label), // Shortened version for display
                    subSectionName
                )).ToList();

                structure.AddSection(mainSectionName, columns);
            }

            return structure;
        }

        private int SetupStructuredHeaders(IXLWorksheet worksheet, HeaderStructure headerStructure)
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
                        // Row 3: Field names (shortened)
                        worksheet.Cell(3, currentCol).Value = column.ShortName;
                        worksheet.Cell(3, currentCol).Style.Font.Bold = true;
                        worksheet.Cell(3, currentCol).Style.Alignment.WrapText = true;
                        worksheet.Cell(3, currentCol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                        currentCol++;
                    }

                    // Row 2: Sub-section headers (if applicable)
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

                // Row 1: Main section headers
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
            // Style header rows
            var headerRange = worksheet.Range(1, 1, 3, totalColumns);
            headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thick;
            headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            headerRange.Style.Font.Bold = true;

            // Style data rows
            if (totalRows > 3)
            {
                var dataRange = worksheet.Range(4, 1, totalRows, totalColumns);
                dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Hair;

                // Alternate row colors
                for (int row = 4; row <= totalRows; row++)
                {
                    if ((row - 4) % 2 == 0)
                    {
                        worksheet.Range(row, 1, row, totalColumns).Style.Fill.BackgroundColor = XLColor.AliceBlue;
                    }
                }
            }

            // Auto-fit columns with limits
            worksheet.Columns().AdjustToContents();
            foreach (var column in worksheet.Columns(1, totalColumns))
            {
                if (column.Width > 40) column.Width = 40;
                if (column.Width < 12) column.Width = 12;
            }

            // Set header row heights
            worksheet.Row(1).Height = 25;
            worksheet.Row(2).Height = 20;
            worksheet.Row(3).Height = 30;
        }

        private void AddSummarySection(IXLWorksheet worksheet, ProcessedCampData data, int startRow)
        {
            worksheet.Cell(startRow, 1).Value = "Export Summary:";
            worksheet.Cell(startRow, 1).Style.Font.Bold = true;
            worksheet.Cell(startRow, 1).Style.Font.FontSize = 12;

            worksheet.Cell(startRow + 1, 1).Value = $"Camp: {data.CampName}";
            worksheet.Cell(startRow + 2, 1).Value = $"Organization: {data.OrganizationName}";
            worksheet.Cell(startRow + 3, 1).Value = $"Total Participants: {data.ParticipantResponses.Count}";
            worksheet.Cell(startRow + 4, 1).Value = $"Total Intake Form Fields: {data.IntakeFieldCount}";
            worksheet.Cell(startRow + 5, 1).Value = $"Total Health Assessment Fields: {data.HealthFieldCount}";
            worksheet.Cell(startRow + 6, 1).Value = $"Total Sections: {data.FieldsBySection.Count}";
            worksheet.Cell(startRow + 7, 1).Value = $"Export Date: {DateTime.Now.AddHours(3):yyyy-MM-dd HH:mm:ss}";

            var summaryRange = worksheet.Range(startRow, 1, startRow + 7, 2);
            summaryRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            summaryRange.Style.Fill.BackgroundColor = XLColor.LightYellow;
        }

        private void AddLegendSection(IXLWorksheet worksheet, ProcessedCampData data, int startRow)
        {
            worksheet.Cell(startRow, 1).Value = "Section Color Legend:";
            worksheet.Cell(startRow, 1).Style.Font.Bold = true;

            int legendRow = startRow + 1;
            var uniqueSections = data.FieldsBySection.Select(g => (string)g.Key.SectionName).Distinct().ToList();
            foreach (var sectionName in uniqueSections)
            {
                worksheet.Cell(legendRow, 1).Value = sectionName;
                worksheet.Cell(legendRow, 1).Style.Fill.BackgroundColor = GetSectionColor(sectionName);
                worksheet.Cell(legendRow, 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                legendRow++;
            }
        }

        #region Helper Methods
        private static int GetFieldPriority(string fieldLabel)
        {
            // Define field-level priority within sections for better organization
            var label = fieldLabel.ToLowerInvariant();

            // Vital Signs ordering
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

            // Physical Examination ordering
            if (label.Contains("rs")) return 1;
            if (label.Contains("cvs")) return 2;
            if (label.Contains("abdomen")) return 3;
            if (label.Contains("cns")) return 4;
            if (label.Contains("skin")) return 5;

            // Eye examination ordering (Right eye first, then Left eye)
            if (label.Contains("right eye")) return 1;
            if (label.Contains("left eye")) return 2;
            if (label.Contains("vision")) return 1;
            if (label.Contains("sphere")) return 2;
            if (label.Contains("cylinder")) return 3;
            if (label.Contains("axis")) return 4;
            if (label.Contains("va")) return 5;
            if (label.Contains("add")) return 6;

            // Diagnosis sections ordering
            if (label.Contains("findings")) return 1;
            if (label.Contains("conclusion")) return 2;
            if (label.Contains("recommendations")) return 3;
            if (label.Contains("additional notes")) return 4;

            // Nutrition ordering
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

            return 50; // Default priority for other fields
        }
        private static XLColor GetSectionColor(string sectionName)
        {
            return sectionName.ToLowerInvariant() switch
            {
                var s when s.Contains("personal") || s.Contains("demographic") => XLColor.LightBlue,
                var s when s.Contains("medical") || s.Contains("health") => XLColor.LightGreen,
                var s when s.Contains("assessment") || s.Contains("evaluation") => XLColor.LightYellow,
                var s when s.Contains("vitals") || s.Contains("vital") => XLColor.LightPink,
                var s when s.Contains("lab") || s.Contains("laboratory") => XLColor.LightCyan,
                var s when s.Contains("history") || s.Contains("background") => XLColor.Lavender,
                var s when s.Contains("physical") || s.Contains("examination") => XLColor.LightSalmon,
                var s when s.Contains("mental") || s.Contains("psychological") => XLColor.LightSteelBlue,
                var s when s.Contains("nutrition") || s.Contains("dietary") => XLColor.LightGoldenrodYellow,
                var s when s.Contains("recommendation") || s.Contains("advice") => XLColor.LightSeaGreen,
                "general" => XLColor.WhiteSmoke,
                _ => XLColor.LightGray
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
        private static XLColor GetLifestyleRiskColor(string risk) => risk switch { "Very Low" => XLColor.LightGreen, "Low" => XLColor.LightYellow, "Medium" => XLColor.Orange, "High" => XLColor.LightCoral, "Very High" => XLColor.Red, _ => XLColor.LightGray };
        private string GetMainSectionName(string fullSectionName) => fullSectionName.Contains("Health Assessment") ? "Health Assessments" : fullSectionName.Split('-')[0].Trim();
        private string GetSubSectionName(string fullSectionName)
        {
            if (fullSectionName.Contains("Health Assessment"))
            {
                var parts = fullSectionName.Split(" - ");
                return parts.Length >= 3 ? $"{parts[1]} - {parts[2]}" : parts.Length > 1 ? parts[1] : fullSectionName;
            }
            return fullSectionName;
        }
        private string TruncateFieldName(string fieldName)
        {
            var replacements = new Dictionary<string, string> { { "Do you have", "Have" }, { "Have you ever", "Ever" }, { "Are you currently", "Currently" }, { "Please specify", "Specify" }, { "How many", "Count" }, { "What is your", "Your" }, { "Date of Birth", "DOB" }, { "Blood Pressure", "BP" }, { "Heart Rate", "HR" }, { "Body Mass Index", "BMI" }, { "Temperature", "Temp" }, { "medication", "meds" }, { "family history", "family hist" }, { "medical history", "med hist" } };
            string shortened = fieldName;
            foreach (var r in replacements) shortened = shortened.Replace(r.Key, r.Value, StringComparison.OrdinalIgnoreCase);
            return shortened.Replace("  ", " ").Trim();
        }
        private XLColor GetMainSectionColor(string sectionName) => sectionName.ToLowerInvariant() switch { "participant information" => XLColor.LightGray, "health assessments" => XLColor.LightBlue, "personal information" => XLColor.LightGreen, "medical history" => XLColor.LightCoral, "lifestyle" => XLColor.LightYellow, "emergency contact" => XLColor.LightPink, _ => XLColor.LightCyan };
        private string FormatFieldValue(string value, string fieldType) => fieldType.ToLowerInvariant() switch { "checkbox" => value == "true" ? "Yes" : value == "false" ? "No" : value, "date" => DateTime.TryParse(value, out var d) ? d.ToString("yyyy-MM-dd") : value, "datetime" => DateTime.TryParse(value, out var dt) ? dt.ToString("yyyy-MM-dd HH:mm") : value, "number" => decimal.TryParse(value, out var n) ? n.ToString("0.##") : value, _ => value };
        #endregion

        #region Header Classes
        public class HeaderStructure
        {
            public List<HeaderSection> Sections { get; set; } = new();
            public void AddSection(string name, List<HeaderColumn> columns)
            {
                var section = Sections.FirstOrDefault(s => s.Name == name);
                if (section == null) { section = new HeaderSection { Name = name }; Sections.Add(section); }
                foreach (var group in columns.GroupBy(c => c.SubSection ?? "General"))
                {
                    section.SubSections.Add(new HeaderSubSection { Name = group.Key == "General" ? "" : group.Key, Columns = group.ToList() });
                }
            }
        }
        public class HeaderSection { public string Name { get; set; } = ""; public List<HeaderSubSection> SubSections { get; set; } = new(); }
        public class HeaderSubSection { public string Name { get; set; } = ""; public List<HeaderColumn> Columns { get; set; } = new(); }
        public class HeaderColumn
        {
            public string FullName { get; set; }
            public string ShortName { get; set; }
            public string? SubSection { get; set; }
            public HeaderColumn(string fullName, string shortName, string? subSection = null) { FullName = fullName; ShortName = shortName; SubSection = subSection; }
        }
        #endregion
    }
}
