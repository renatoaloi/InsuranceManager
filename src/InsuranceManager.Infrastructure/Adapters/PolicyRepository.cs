using Microsoft.EntityFrameworkCore;
using InsuranceManager.Domain.Entities;
using InsuranceManager.Domain.Ports;
using InsuranceManager.Infrastructure.Persistence;

namespace InsuranceManager.Infrastructure.Adapters;

public class PolicyRepository : IPolicyRepository
{
    private readonly InsuranceDbContext _ctx;

    public PolicyRepository(InsuranceDbContext ctx) => _ctx = ctx;

    public async Task<Policy?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _ctx.Policies.FindAsync(new object[] { id }, ct);

    public async Task<IReadOnlyList<Policy>> GetAllAsync(CancellationToken ct = default)
        => await _ctx.Policies.ToListAsync(ct);

    public async Task<Policy> AddAsync(Policy policy, CancellationToken ct = default)
    {
        _ctx.Policies.Add(policy);
        await _ctx.SaveChangesAsync(ct);
        return policy;
    }
}