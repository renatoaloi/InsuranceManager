using InsuranceManager.Domain.ValueObjects;

namespace InsuranceManager.Api.DTOs;

public class ChangeProposalStatusDto
{
    public ProposalStatus Status { get; init; }
}