using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Salubrity.Domain.Entities.HealthAssesment;

namespace Salubrity.Infrastructure.Persistence.Configurations.HealthAssessment;

public class HealthAssessmentRecommendationConfiguration : IEntityTypeConfiguration<HealthAssessmentRecommendation>
{
    public void Configure(EntityTypeBuilder<HealthAssessmentRecommendation> b)
    {
        b.HasKey(x => x.Id);

        b.HasOne(x => x.HealthAssessment)
            .WithMany(a => a.Recommendations)
            .HasForeignKey(x => x.HealthAssessmentId)
            .OnDelete(DeleteBehavior.Cascade);

        b.Property(x => x.Title).HasMaxLength(200).IsRequired();
        b.Property(x => x.Description).HasMaxLength(1000);
        b.Property(x => x.Priority).HasMaxLength(30);
    }
}
