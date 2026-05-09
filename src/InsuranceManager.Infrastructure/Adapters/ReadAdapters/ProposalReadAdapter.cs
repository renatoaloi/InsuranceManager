using Microsoft.EntityFrameworkCore;
using InsuranceManager.Domain.Ports;
using InsuranceManager.Domain.ValueObjects;
using InsuranceManager.Infrastructure.Persistence;

namespace InsuranceManager.Infrastructure.Adapters.ReadAdapters;

public class ProposalReadAdapter : IProposalReadAdapter
{
    private readonly InsuranceDbContext _ctx;

    public ProposalReadAdapter(InsuranceDbContext ctx) => _ctx = ctx;

    public async Task<IReadOnlyList<ProposalListItem>> GetAllAsync(
        ProposalStatus? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken ct = default)
    {
        var query = _ctx.Proposals.AsQueryable();

        if (status.HasValue)
            query = query.Where(p => p.Status == status.Value);

        if (fromDate.HasValue)
            query = query.Where(p => p.CreatedAt >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(p => p.CreatedAt <= toDate.Value);

        var results = await query
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new ProposalListItem(
                p.Id,
                p.ClientName,
                p.CoverageType,
                p.Status,
                p.CreatedAt,
                p.UpdatedAt))
            .ToListAsync(ct);

        return results;
    }

    public async Task<ProposalListItem?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _ctx.Proposals
            .Where(p => p.Id == id)
            .Select(p => new ProposalListItem(
                p.Id, p.ClientName, p.CoverageType, p.Status, p.CreatedAt, p.UpdatedAt))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<int> GetCountByStatusAsync(ProposalStatus status, CancellationToken ct = default)
        => await _ctx.Proposals.CountAsync(p => p.Status == status, ct);
}