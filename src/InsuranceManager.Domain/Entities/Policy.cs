using InsuranceManager.Domain.ValueObjects;

namespace InsuranceManager.Domain.Entities;

public class Policy
{
    public Guid Id { get; private set; }
    public Guid ProposalId { get; private set; }
    public string InsuredAssetValue { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }

    public AssetToken InsuredAsset => new AssetToken(InsuredAssetValue);

    private Policy() { }

    public static Policy CreateFromApprovedProposal(Proposal proposal)
    {
        if (!proposal.CanBeContracted())
            throw new InvalidOperationException("Only approved proposals can be contracted");

        return new Policy
        {
            Id = Guid.NewGuid(),
            ProposalId = proposal.Id,
            InsuredAssetValue = AssetToken.Generate().Value,
            CreatedAt = DateTime.UtcNow
        };
    }
}