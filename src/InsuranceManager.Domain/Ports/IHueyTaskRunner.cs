using InsuranceManager.Domain.ValueObjects;

namespace InsuranceManager.Domain.Ports;

public interface IHueyTaskRunner
{
    Task EnqueueStatusChangeAsync(Guid proposalId, ProposalStatus newStatus, CancellationToken ct = default);
}