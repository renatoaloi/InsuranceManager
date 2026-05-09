using InsuranceManager.Domain.Entities;
using InsuranceManager.Domain.ValueObjects;

namespace InsuranceManager.Api.DTOs;

public record ProposalResponseDto(
    Guid Id,
    string ClientName,
    CoverageType CoverageType,
    ProposalStatus Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public static class ProposalResponseDtoExtensions
{
    public static ProposalResponseDto ToDto(this Proposal proposal)
        => new(proposal.Id, proposal.ClientName, proposal.CoverageType, proposal.Status, 
               proposal.CreatedAt, proposal.UpdatedAt);
}