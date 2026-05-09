using InsuranceManager.Domain.Entities;
using InsuranceManager.Domain.ValueObjects;

namespace InsuranceManager.Domain.Ports;

public interface IProposalRepository
{
    Task<Proposal?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Proposal>> GetAllAsync(ProposalStatus? status = null, CancellationToken ct = default);
    Task<Proposal> AddAsync(Proposal proposal, CancellationToken ct = default);
    Task UpdateAsync(Proposal proposal, CancellationToken ct = default);
}