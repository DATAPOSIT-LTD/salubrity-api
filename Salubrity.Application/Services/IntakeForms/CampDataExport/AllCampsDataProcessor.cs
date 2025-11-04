using Salubrity.Application.DTOs.Clinical;
using Salubrity.Application.DTOs.Forms.IntakeFormResponses;
using Salubrity.Application.DTOs.HealthAssessments;
using Salubrity.Domain.Entities.Identity;
using Salubrity.Domain.Entities.IntakeForms;

namespace Salubrity.Application.Services.IntakeForms.CampDataExport
{
    public class AllCampsDataProcessor
    {
        public ProcessedAllCampsData Process(AllCampsData data)
        {
            var allIntakeFormFields = new List<FieldDefinition>();
            var allHealthAssessmentFields = new List<FieldDefinition>();
            var allDoctorRecommendationFields = BuildDoctorRecommendationFields();

            var allParticipantResponses = new List<ParticipantResponseWithCampInfo>();
            var allDtoResponseLookup = new Dictionary<Guid, List<IntakeFormResponseDetailDto>>();
            var allHealthAssessmentLookup = new Dictionary<Guid, Dictionary<string, string>>();
            var allDoctorRecommendationLookup = new Dictionary<Guid, DoctorRecommendationResponseDto?>();

            var existingIntakeFieldIds = new HashSet<string>();
            var existingHealthFieldIds = new HashSet<string>();

            // Process each camp's data
            foreach (var campData in data.CampDataList)
            {
                // Build intake form fields from this camp
                var campIntakeFormFields = BuildIntakeFormFields(campData.DtoResponses, campData.EntityResponses, existingIntakeFieldIds);
                allIntakeFormFields.AddRange(campIntakeFormFields);

                // Build health assessment fields from this camp
                var campHealthAssessmentFields = BuildHealthAssessmentFields(campData.HealthAssessmentResponses, existingHealthFieldIds);
                allHealthAssessmentFields.AddRange(campHealthAssessmentFields);

                // Process participant responses for this camp
                var campParticipantResponses = campData.EntityResponses
                    .GroupBy(r => r.PatientId)
                    .Where(g => g.First().Patient != null)
                    .Select(g => new ParticipantResponseWithCampInfo
                    {
                        PatientId = g.Key,
                        Patient = g.First().Patient!,
                        Responses = g.ToList(),
                        OrganizationName = campData.OrganizationName,
                        CampName = campData.Camp.Name,
                        CampDate = campData.CampDate
                    })
                    .ToList();

                allParticipantResponses.AddRange(campParticipantResponses);

                // Merge DTO response lookup
                var campDtoResponseLookup = campData.DtoResponses
                    .GroupBy(r => r.PatientId)
                    .ToDictionary(g => g.Key, g => g.ToList());

                foreach (var kvp in campDtoResponseLookup)
                {
                    if (allDtoResponseLookup.ContainsKey(kvp.Key))
                    {
                        allDtoResponseLookup[kvp.Key].AddRange(kvp.Value);
                    }
                    else
                    {
                        allDtoResponseLookup[kvp.Key] = kvp.Value;
                    }
                }

                // Merge health assessment lookup
                var campHealthAssessmentLookup = campData.HealthAssessmentResponses
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value
                            .SelectMany(assessment => assessment.Sections
                                .SelectMany(section => section.Fields
                                    .Select(field => new { assessment.FormName, section.SectionName, field })))
                            .ToDictionary(
                                item => $"health_{item.FormName}_{item.SectionName}_{item.field.FieldLabel}".Replace(" ", "_"),
                                item => item.field.Value ?? item.field.SelectedOption ?? ""
                            )
                    );

                foreach (var kvp in campHealthAssessmentLookup)
                {
                    allHealthAssessmentLookup[kvp.Key] = kvp.Value;
                }

                // Merge doctor recommendation lookup
                var campDoctorRecommendationLookup = campData.DoctorRecommendations
                    .GroupBy(r => r.PatientId)
                    .ToDictionary(g => g.Key, g => g.OrderByDescending(r => r.CreatedAt).FirstOrDefault());

                foreach (var kvp in campDoctorRecommendationLookup)
                {
                    allDoctorRecommendationLookup[kvp.Key] = kvp.Value;
                }
            }

            // Combine all fields
            var allFields = allIntakeFormFields.Concat(allHealthAssessmentFields).Concat(allDoctorRecommendationFields).ToList();

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

            // Sort participants by camp date, then by name
            var sortedParticipantResponses = allParticipantResponses
                .OrderBy(p => p.CampDate)
                .ThenBy(p => p.Patient.User?.FirstName ?? "")
                .ToList();

            return new ProcessedAllCampsData
            {
                ParticipantResponses = sortedParticipantResponses,
                DtoResponseLookup = allDtoResponseLookup,
                HealthAssessmentLookup = allHealthAssessmentLookup,
                DoctorRecommendationLookup = allDoctorRecommendationLookup,
                OrderedFields = orderedFields,
                FieldsBySection = fieldsBySection,
                IntakeFieldCount = allIntakeFormFields.Count,
                HealthFieldCount = allHealthAssessmentFields.Count,
                DoctorRecommendationFieldCount = allDoctorRecommendationFields.Count,
                TotalCamps = data.CampDataList.Count,
                TotalParticipants = sortedParticipantResponses.Count
            };
        }

        private List<FieldDefinition> BuildIntakeFormFields(List<IntakeFormResponseDetailDto> dtoResponses, List<IntakeFormResponse> entityResponses, HashSet<string> existingFieldIds)
        {
            var intakeFormFields = new List<FieldDefinition>();

            // Process all DTO responses from all participants
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

            // Also process all entity responses to catch any fields not present in DTOs
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

            return intakeFormFields;
        }

        private List<FieldDefinition> BuildHealthAssessmentFields(Dictionary<Guid, List<HealthAssessmentResponseDto>> healthAssessmentResponses, HashSet<string> existingFieldIds)
        {
            var healthAssessmentFields = new List<FieldDefinition>();

            // Iterate through all patient assessments to build a complete list of fields
            foreach (var patientAssessments in healthAssessmentResponses.Values)
            {
                foreach (var assessmentResponse in patientAssessments)
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

        private List<FieldDefinition> BuildDoctorRecommendationFields()
        {
            return new List<FieldDefinition>
            {
                new("doctor_pertinent_history", "Pertinent History Findings", "Doctor Recommendations", 1, "text", "DoctorRecommendation", 1500),
                new("doctor_pertinent_clinical", "Pertinent Clinical Findings", "Doctor Recommendations", 2, "text", "DoctorRecommendation", 1500),
                new("doctor_diagnostic_impression", "Diagnostic Impression", "Doctor Recommendations", 3, "text", "DoctorRecommendation", 1500),
                new("doctor_conclusion", "Conclusion", "Doctor Recommendations", 4, "text", "DoctorRecommendation", 1500),
                new("doctor_followup_recommendation", "Follow-up Recommendation", "Doctor Recommendations", 5, "text", "DoctorRecommendation", 1500),
                new("doctor_recommendation_type", "Recommendation Type", "Doctor Recommendations", 6, "text", "DoctorRecommendation", 1500),
                new("doctor_instructions", "Instructions", "Doctor Recommendations", 7, "text", "DoctorRecommendation", 1500),
            };
        }

        private static int GetSectionPriority(string sectionName, string fieldLabel)
        {
            return sectionName.ToLowerInvariant() switch
            {
                var s when s.Contains("diagnosis") && s.Contains("examination") => 100,
                var s when s.Contains("physical examination") => 200,
                var s when s.Contains("eye") || s.Contains("vision") => 300,
                var s when s.Contains("nutrition") => 400,
                var s when s.Contains("mental health") => 500,
                var s when s.Contains("back") || s.Contains("muscle") || s.Contains("nerve") || s.Contains("bone") => 510,
                var s when s.Contains("well woman") => 520,
                var s when s.Contains("findings") => 600,
                var s when s.Contains("diagnosis") && !s.Contains("examination") => 610,
                var s when s.Contains("notes") => 700,
                var s when s.Contains("recommendations") => 800,
                _ => 900
            };
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
    }

    public class ParticipantResponseWithCampInfo
    {
        public Guid PatientId { get; set; }
        public Patient Patient { get; set; } = null!;
        public List<IntakeFormResponse> Responses { get; set; } = [];
        public string OrganizationName { get; set; } = string.Empty;
        public string CampName { get; set; } = string.Empty;
        public DateTime CampDate { get; set; }
    }

    public class ProcessedAllCampsData
    {
        public List<ParticipantResponseWithCampInfo> ParticipantResponses { get; set; } = [];
        public Dictionary<Guid, List<IntakeFormResponseDetailDto>> DtoResponseLookup { get; set; } = [];
        public Dictionary<Guid, Dictionary<string, string>> HealthAssessmentLookup { get; set; } = [];
        public Dictionary<Guid, DoctorRecommendationResponseDto?> DoctorRecommendationLookup { get; set; } = [];
        public List<FieldDefinition> OrderedFields { get; set; } = [];
        public List<IGrouping<dynamic, FieldDefinition>> FieldsBySection { get; set; } = [];
        public int IntakeFieldCount { get; set; }
        public int HealthFieldCount { get; set; }
        public int DoctorRecommendationFieldCount { get; set; }
        public int TotalCamps { get; set; }
        public int TotalParticipants { get; set; }
    }
}