using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Salubrity.Domain.Entities.Organizations;

namespace Salubrity.Infrastructure.Persistence.Configurations.Organizations;

public class OrganizationBranchConfiguration : IEntityTypeConfiguration<OrganizationBranch>
{
    public void Configure(EntityTypeBuilder<OrganizationBranch> builder)
    {
        builder.ToTable("OrganizationBranches");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.BranchName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(b => b.Code)
            .HasMaxLength(50);

        builder.Property(b => b.Address)
            .HasMaxLength(300);

        builder.Property(b => b.City)
            .HasMaxLength(150);

        builder.Property(b => b.Region)
            .HasMaxLength(150);

        builder.Property(b => b.ContactEmail)
            .HasMaxLength(200);

        builder.Property(b => b.ContactPhone)
            .HasMaxLength(50);

        builder.HasIndex(b => new { b.OrganizationId, b.BranchName })
            .IsUnique()
            .HasDatabaseName("UX_BranchName_Per_Org");

        builder
            .HasOne(b => b.Organization)
            .WithMany(o => o.Branches)
            .HasForeignKey(b => b.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
