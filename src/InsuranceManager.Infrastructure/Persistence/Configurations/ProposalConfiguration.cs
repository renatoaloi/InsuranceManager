using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InsuranceManager.Domain.Entities;
using InsuranceManager.Domain.ValueObjects;

namespace InsuranceManager.Infrastructure.Persistence.Configurations;

public class ProposalConfiguration : IEntityTypeConfiguration<Proposal>
{
    public void Configure(EntityTypeBuilder<Proposal> builder)
    {
        builder.ToTable("Proposals");
        
        builder.HasKey(p => p.Id);
        
        builder.Property(p => p.ClientName)
            .IsRequired()
            .HasMaxLength(200);
        
        builder.Property(p => p.CoverageType)
            .HasConversion<int>()
            .IsRequired();
        
        builder.Property(p => p.Status)
            .HasConversion<int>()
            .IsRequired();
        
        builder.Property(p => p.CreatedAt)
            .IsRequired();
        
        builder.Property(p => p.UpdatedAt)
            .IsRequired(false);
    }
}