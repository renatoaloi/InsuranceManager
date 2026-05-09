using InsuranceManager.Domain.Entities;

namespace InsuranceManager.Api.DTOs;

public record PolicyResponseDto(
    Guid Id,
    Guid ProposalId,
    string InsuredAsset,
    DateTime CreatedAt
);

public static class PolicyResponseDtoExtensions
{
    public static PolicyResponseDto ToDto(this Policy policy)
        => new(policy.Id, policy.ProposalId, policy.InsuredAsset.Value, policy.CreatedAt);
}