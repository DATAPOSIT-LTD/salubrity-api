using System;
using Salubrity.Domain.Common;
using Salubrity.Domain.Entities.IntakeForms;

namespace Salubrity.Domain.Entities.Reporting
{
    public class ReportingMetricMapping : BaseAuditableEntity
    {
        /// <summary>
        /// The form field that captures the value
        /// </summary>
        public Guid FieldId { get; set; }

        /// <summary>
        /// Stable name used in reporting (e.g., has_tb_history, smoker, bmi)
        /// </summary>
        public string MetricCode { get; set; } = null!;

        /// <summary>
        /// Human-readable label (optional for admin clarity)
        /// </summary>
        public string Label { get; set; } = null!;

        /// <summary>
        /// Type of the response data (boolean, text, integer, decimal)
        /// </summary>
        public string DataType { get; set; } = "boolean";

        // Navigation
        public IntakeFormField Field { get; set; } = null!;
    }
}
