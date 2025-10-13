using Salubrity.Application.DTOs.Forms.IntakeFormResponses;
using Salubrity.Application.DTOs.HealthAssessments;
using Salubrity.Domain.Entities.IntakeForms;

namespace Salubrity.Application.Services.IntakeForms.CampDataExport
{
    public class CampDataProcessor
    {
        public ProcessedCampData Process(CampData data)
        {
            var intakeFormFields = BuildIntakeFormFields(data.DtoResponses, data.EntityResponses);
            var healthAssessmentFields = BuildHealthAssessmentFields(data.HealthAssessmentResponses);

            var allFields = intakeFormFields.Concat(healthAssessmentFields).ToList();

            var fieldsBySection = allFields
                .GroupBy(f => new { f.SectionName, f.SectionPriority })
                .OrderBy(g => g.Key.SectionPriority)
                .ThenBy(g => g.Key.SectionName)
                .ToList<IGrouping<dynamic, FieldDefinition>>();

            var orderedFields = new List<FieldDefinition>();
            foreach (var sectionGroup in fieldsBySection)
            {
                var fieldsInSection = sectionGroup.OrderBy(f => GetFieldPriority(f.Label)).ThenBy(f => f.Order);
                orderedFields.AddRange(fieldsInSection);
            }

            var participantResponses = data.EntityResponses
                .GroupBy(r => r.PatientId)
                .Where(g => g.First().Patient != null)
                .OrderBy(g => g.First().Patient?.User?.FirstName ?? "")
                .ToList();

            var dtoResponseLookup = data.DtoResponses
                .GroupBy(r => r.PatientId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var healthAssessmentLookup = data.HealthAssessmentResponses
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value
                        .SelectMany(assessment => assessment.Sections
                            .SelectMany(section => section.Fields
                                .Select(field => new { section.SectionName, field })))
                        .ToDictionary(
                            item => $"health_{kvp.Value.First().FormName}_{item.SectionName}_{item.field.FieldLabel}".Replace(" ", "_"),
                            item => item.field.Value ?? item.field.SelectedOption ?? ""
                        )
                );

            return new ProcessedCampData
            {
                CampName = data.Camp.Name,
                OrganizationName = data.OrganizationName,
                ParticipantResponses = participantResponses,
                DtoResponseLookup = dtoResponseLookup,
                HealthAssessmentLookup = healthAssessmentLookup,
                OrderedFields = orderedFields,
                FieldsBySection = fieldsBySection,
                IntakeFieldCount = intakeFormFields.Count,
                HealthFieldCount = healthAssessmentFields.Count
            };
        }

        private List<FieldDefinition> BuildIntakeFormFields(List<IntakeFormResponseDetailDto> dtoResponses, List<IntakeFormResponse> entityResponses)
        {
            var intakeFormFields = new List<FieldDefinition>();
            var existingFieldIds = new HashSet<string>();

            // Prefer DTO responses for field structure
            foreach (var fieldResponse in dtoResponses.SelectMany(r => r.FieldResponses))
            {
                var fieldId = $"intake_{fieldResponse.FieldId}";
                if (existingFieldIds.Add(fieldId))
                {
                    var sectionName = fieldResponse.Field.SectionName ?? "General";
                    intakeFormFields.Add(new FieldDefinition
                    (
                        FieldId: fieldId,
                        Label: fieldResponse.Field.Label,
                        SectionName: sectionName,
                        Order: fieldResponse.Field.Order,
                        FieldType: fieldResponse.Field.FieldType ?? "text",
                        DataSource: "IntakeForm",
                        SectionPriority: GetSectionPriority(sectionName, fieldResponse.Field.Label)
                    ));
                }
            }

            // Fallback to entity responses if no DTOs
            if (!intakeFormFields.Any())
            {
                foreach (var fieldResponse in entityResponses.SelectMany(r => r.FieldResponses))
                {
                    var fieldId = $"intake_{fieldResponse.FieldId}";
                    if (existingFieldIds.Add(fieldId))
                    {
                        var sectionName = fieldResponse.Field.Section?.Name ?? "General";
                        intakeFormFields.Add(new FieldDefinition
                        (
                            FieldId: fieldId,
                            Label: fieldResponse.Field.Label,
                            SectionName: sectionName,
                            Order: fieldResponse.Field.Order,
                            FieldType: fieldResponse.Field.FieldType ?? "text",
                            DataSource: "IntakeForm",
                            SectionPriority: GetSectionPriority(sectionName, fieldResponse.Field.Label)
                        ));
                    }
                }
            }

            return intakeFormFields;
        }

        private List<FieldDefinition> BuildHealthAssessmentFields(Dictionary<Guid, List<HealthAssessmentResponseDto>> healthAssessmentResponses)
        {
            var healthAssessmentFields = new List<FieldDefinition>();
            var existingFieldIds = new HashSet<string>();
            var sampleAssessment = healthAssessmentResponses.Values.FirstOrDefault();

            if (sampleAssessment != null)
            {
                foreach (var assessmentResponse in sampleAssessment)
                {
                    foreach (var section in assessmentResponse.Sections)
                    {
                        foreach (var field in section.Fields)
                        {
                            var fieldId = $"health_{assessmentResponse.FormName}_{section.SectionName}_{field.FieldLabel}".Replace(" ", "_");
                            if (existingFieldIds.Add(fieldId))
                            {
                                healthAssessmentFields.Add(new FieldDefinition
                                (
                                    FieldId: fieldId,
                                    Label: field.FieldLabel,
                                    SectionName: $"Health Assessment - {assessmentResponse.FormName} - {section.SectionName}",
                                    Order: field.FieldOrder,
                                    FieldType: "text",
                                    DataSource: "HealthAssessment",
                                    SectionPriority: 1000
                                ));
                            }
                        }
                    }
                }
            }

            return healthAssessmentFields;
        }

        private static int GetSectionPriority(string sectionName, string fieldLabel)
        {
            // Define the structured ordering based on our proposed arrangement
            return sectionName.ToLowerInvariant() switch
            {
                var s when s.Contains("diagnosis") && s.Contains("examination") => 100, // Physical Measurements & Vital Signs
                var s when s.Contains("physical examination") => 200, // Physical Examination
                var s when s.Contains("eye") || s.Contains("vision") => 300, // Eye Examination
                var s when s.Contains("nutrition") => 400, // Nutrition Assessment
                var s when s.Contains("mental health") => 500, // Mental Health
                var s when s.Contains("back") || s.Contains("muscle") || s.Contains("nerve") || s.Contains("bone") => 510, // Back, Muscles, Nerves & Bones
                var s when s.Contains("well woman") => 520, // Well Woman
                var s when s.Contains("findings") => 600, // General Findings
                var s when s.Contains("diagnosis") && !s.Contains("examination") => 610, // General Diagnosis
                var s when s.Contains("notes") => 700, // Notes
                var s when s.Contains("recommendations") => 800, // Recommendations
                _ => 900 // Other/General sections
            };
        }

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
    }

    public record FieldDefinition(string FieldId, string Label, string SectionName, int Order, string FieldType, string DataSource, int SectionPriority);

    public class ProcessedCampData
    {
        public string CampName { get; set; } = string.Empty;
        public string OrganizationName { get; set; } = string.Empty;
        public List<IGrouping<Guid, IntakeFormResponse>> ParticipantResponses { get; set; } = [];
        public Dictionary<Guid, List<IntakeFormResponseDetailDto>> DtoResponseLookup { get; set; } = [];
        public Dictionary<Guid, Dictionary<string, string>> HealthAssessmentLookup { get; set; } = [];
        public List<FieldDefinition> OrderedFields { get; set; } = [];
        public List<IGrouping<dynamic, FieldDefinition>> FieldsBySection { get; set; } = [];
        public int IntakeFieldCount { get; set; }
        public int HealthFieldCount { get; set; }
    }
}
