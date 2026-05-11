using InsuranceManager.Domain.ValueObjects;

namespace InsuranceManager.Domain.Ports;

public interface IQueueTaskAdapter
{
    Task EnqueueStatusChangeAsync(Guid proposalId, ProposalStatus newStatus, CancellationToken ct = default);
}