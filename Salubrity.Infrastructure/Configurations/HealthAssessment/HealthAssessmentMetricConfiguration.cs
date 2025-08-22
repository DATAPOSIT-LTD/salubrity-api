using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Salubrity.Domain.Entities.HealthAssesment;

namespace Salubrity.Infrastructure.Persistence.Configurations.HealthAssessment;

public class HealthAssessmentMetricConfiguration : IEntityTypeConfiguration<HealthAssessmentMetric>
{
    public void Configure(EntityTypeBuilder<HealthAssessmentMetric> b)
    {
        b.HasKey(x => x.Id);

        b.HasOne(x => x.HealthAssessment)
            .WithMany(a => a.Metrics)
            .HasForeignKey(x => x.HealthAssessmentId)
            .OnDelete(DeleteBehavior.Cascade);

        b.Property(x => x.Name).HasMaxLength(150).IsRequired();

        // ReferenceRange removed — it's now obsolete
    }
}
