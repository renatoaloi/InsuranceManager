using Microsoft.EntityFrameworkCore;
using InsuranceManager.Domain.Entities;

namespace InsuranceManager.Infrastructure.Persistence;

public class InsuranceDbContext : DbContext
{
    public DbSet<Proposal> Proposals => Set<Proposal>();
    public DbSet<Policy> Policies => Set<Policy>();

    public InsuranceDbContext(DbContextOptions<InsuranceDbContext> options) 
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(InsuranceDbContext).Assembly);
    }
}