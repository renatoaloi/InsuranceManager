namespace InsuranceManager.Application.Commands;

public record ChangeProposalStatusCommand(Guid ProposalId, InsuranceManager.Domain.ValueObjects.ProposalStatus NewStatus);