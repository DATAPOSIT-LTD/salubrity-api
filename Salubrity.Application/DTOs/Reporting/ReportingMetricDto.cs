using System;

namespace Salubrity.Application.DTOs.Reporting
{
    public class ReportingMetricDto
    {
        public Guid Id { get; set; }
        public Guid FieldId { get; set; }
        public string MetricCode { get; set; } = null!;
        public string Category { get; set; } = null!;
        public string Label { get; set; } = null!;
        public string DataType { get; set; } = "boolean";
    }

    public class CreateReportingMetricDto
    {
        public Guid FieldId { get; set; }
        public string MetricCode { get; set; } = null!;
        public string Category { get; set; } = null!;
        public string Label { get; set; } = null!;
        public string DataType { get; set; } = "boolean";
    }
}
