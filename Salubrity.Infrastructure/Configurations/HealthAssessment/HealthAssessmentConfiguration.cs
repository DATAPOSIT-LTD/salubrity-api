using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Salubrity.Domain.Entities.HealthAssesment;

namespace Salubrity.Infrastructure.Persistence.Configurations.HealthAssessment;

public class HealthAssessmentConfiguration : IEntityTypeConfiguration<Salubrity.Domain.Entities.HealthAssesment.HealthAssessment>
{
    public void Configure(EntityTypeBuilder<Salubrity.Domain.Entities.HealthAssesment.HealthAssessment> b)
    {
        b.HasKey(x => x.Id);

        b.HasOne(x => x.HealthCamp)
            .WithMany(c => c.HealthAssessments)
            .HasForeignKey(x => x.HealthCampId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.Participant)
            .WithMany(p => p.HealthAssessments)
            .HasForeignKey(x => x.ParticipantId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.ReviewedBy)
            .WithMany()
            .HasForeignKey(x => x.ReviewedById)
            .OnDelete(DeleteBehavior.SetNull);

        b.Property(x => x.OverallScore).HasDefaultValue(0);
    }
}
