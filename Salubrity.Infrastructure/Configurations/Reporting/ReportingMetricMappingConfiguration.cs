using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Salubrity.Domain.Entities.Reporting;

namespace Salubrity.Infrastructure.Persistence.Configurations
{
    public class ReportingMetricMappingConfiguration : IEntityTypeConfiguration<ReportingMetricMapping>
    {
        public void Configure(EntityTypeBuilder<ReportingMetricMapping> builder)
        {
            builder.ToTable("ReportingMetricMappings");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.MetricCode)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.Label)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(x => x.DataType)
                .IsRequired()
                .HasMaxLength(50)
                .HasDefaultValue("boolean");

            builder.HasOne(x => x.Field)
                .WithMany()
                .HasForeignKey(x => x.FieldId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
