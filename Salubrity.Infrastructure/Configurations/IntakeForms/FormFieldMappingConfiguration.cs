using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Salubrity.Domain.Entities.IntakeForms;

namespace Salubrity.Infrastructure.Configurations.IntakeForms;

public class FormFieldMappingConfiguration : IEntityTypeConfiguration<FormFieldMapping>
{
    public void Configure(EntityTypeBuilder<FormFieldMapping> builder)
    {
        builder.ToTable("FormFieldMappings");

        builder.HasKey(f => f.Id);

        builder.HasIndex(f => new { f.IntakeFormVersionId, f.Alias }).IsUnique();

        builder.Property(f => f.Alias)
            .IsRequired()
            .HasMaxLength(255);

        builder.HasOne(f => f.IntakeFormVersion)
            .WithMany()
            .HasForeignKey(f => f.IntakeFormVersionId);

        builder.HasOne(f => f.IntakeFormField)
            .WithMany()
            .HasForeignKey(f => f.IntakeFormFieldId);
    }
}
