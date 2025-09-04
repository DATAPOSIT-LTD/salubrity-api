using System;
using System.Collections.Generic;

namespace Salubrity.Application.DTOs.Forms.IntakeFormResponses
{
    public class IntakeFormResponseDetailDto
    {
        public Guid Id { get; set; }
        public Guid IntakeFormVersionId { get; set; }
        public Guid SubmittedByUserId { get; set; }
        public Guid PatientId { get; set; }
        public Guid? ServiceId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public ResponseStatusDto Status { get; set; } = default!;
        public MiniIntakeFormVersionDto Version { get; set; } = default!;
        public MiniServiceDto? Service { get; set; }
        public List<IntakeFormFieldResponseDetailDto> FieldResponses { get; set; } = new();
    }

    public class IntakeFormFieldResponseDetailDto
    {
        public Guid Id { get; set; }
        public Guid FieldId { get; set; }
        public string? Value { get; set; }
        public FieldMetaDto Field { get; set; } = default!;
    }

    public class FieldMetaDto
    {
        public Guid FieldId { get; set; }
        public string Label { get; set; } = default!;
        public string FieldType { get; set; } = default!;
        public Guid? SectionId { get; set; }
        public string? SectionName { get; set; }
        public int Order { get; set; }
    }

    public class ResponseStatusDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
    }

    public class MiniServiceDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
    }

    public class MiniIntakeFormVersionDto
    {
        public Guid Id { get; set; }
        public Guid IntakeFormId { get; set; }
        public int? VersionNumber { get; set; }
        public string IntakeFormName { get; set; } = default!;
        public string? IntakeFormDescription { get; set; }
    }
}
