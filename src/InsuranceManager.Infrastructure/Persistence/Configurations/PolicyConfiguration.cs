using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InsuranceManager.Domain.Entities;

namespace InsuranceManager.Infrastructure.Persistence.Configurations;

public class PolicyConfiguration : IEntityTypeConfiguration<Policy>
{
    public void Configure(EntityTypeBuilder<Policy> builder)
    {
        builder.ToTable("Policies");
        
        builder.HasKey(p => p.Id);
        
        builder.Property(p => p.ProposalId)
            .IsRequired();
        
        builder.Property(p => p.InsuredAssetValue)
            .IsRequired()
            .HasMaxLength(32)
            .HasColumnName("InsuredAsset");
        
        builder.Property(p => p.CreatedAt)
            .IsRequired();
        
        builder.HasOne<Proposal>()
            .WithMany()
            .HasForeignKey(p => p.ProposalId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}