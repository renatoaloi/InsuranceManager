using InsuranceManager.Domain.ValueObjects;

namespace InsuranceManager.Domain.Ports;

public record ProposalListItem(
    Guid Id,
    string ClientName,
    CoverageType CoverageType,
    ProposalStatus Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public interface IProposalReadAdapter
{
    Task<IReadOnlyList<ProposalListItem>> GetAllAsync(
        ProposalStatus? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken ct = default);

    Task<ProposalListItem?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<int> GetCountByStatusAsync(ProposalStatus status, CancellationToken ct = default);
}