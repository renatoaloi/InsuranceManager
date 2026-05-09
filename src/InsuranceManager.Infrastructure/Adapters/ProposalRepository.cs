using Microsoft.EntityFrameworkCore;
using InsuranceManager.Domain.Entities;
using InsuranceManager.Domain.Ports;
using InsuranceManager.Domain.ValueObjects;
using InsuranceManager.Infrastructure.Persistence;

namespace InsuranceManager.Infrastructure.Adapters;

public class ProposalRepository : IProposalRepository
{
    private readonly InsuranceDbContext _ctx;

    public ProposalRepository(InsuranceDbContext ctx) => _ctx = ctx;

    public async Task<Proposal?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _ctx.Proposals.FindAsync(new object[] { id }, ct);

    public async Task<IReadOnlyList<Proposal>> GetAllAsync(
        ProposalStatus? status = null, 
        CancellationToken ct = default)
    {
        var query = _ctx.Proposals.AsQueryable();
        if (status.HasValue)
            query = query.Where(p => p.Status == status.Value);
        return await query.ToListAsync(ct);
    }

    public async Task<Proposal> AddAsync(Proposal proposal, CancellationToken ct = default)
    {
        _ctx.Proposals.Add(proposal);
        await _ctx.SaveChangesAsync(ct);
        return proposal;
    }

    public async Task UpdateAsync(Proposal proposal, CancellationToken ct = default)
    {
        _ctx.Proposals.Update(proposal);
        await _ctx.SaveChangesAsync(ct);
    }
}