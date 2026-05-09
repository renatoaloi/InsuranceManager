using InsuranceManager.Domain.ValueObjects;

namespace InsuranceManager.Domain.Entities;

public class Proposal
{
    public Guid Id { get; private set; }
    public string ClientName { get; private set; } = string.Empty;
    public CoverageType CoverageType { get; private set; }
    public ProposalStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private Proposal() { }

    public static Proposal Create(string clientName, CoverageType coverageType)
    {
        if (string.IsNullOrWhiteSpace(clientName))
            throw new ArgumentException("Client name is required", nameof(clientName));

        return new Proposal
        {
            Id = Guid.NewGuid(),
            ClientName = clientName,
            CoverageType = coverageType,
            Status = ProposalStatus.EmAnalise,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Approve()
    {
        if (Status != ProposalStatus.EmAnalise)
            throw new InvalidOperationException("Only proposals in 'Em Analise' status can be approved");
        
        Status = ProposalStatus.Aprovada;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Reject()
    {
        if (Status != ProposalStatus.EmAnalise)
            throw new InvalidOperationException("Only proposals in 'Em Analise' status can be rejected");
        
        Status = ProposalStatus.Recusada;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool CanTransitionTo(ProposalStatus targetStatus)
    {
        return (Status, targetStatus) switch
        {
            (ProposalStatus.EmAnalise, ProposalStatus.Aprovada) => true,
            (ProposalStatus.EmAnalise, ProposalStatus.Recusada) => true,
            _ => false
        };
    }

    public bool CanBeContracted() => Status == ProposalStatus.Aprovada;
}