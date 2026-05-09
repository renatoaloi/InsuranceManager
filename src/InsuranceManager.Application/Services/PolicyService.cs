using InsuranceManager.Application.Commands;
using InsuranceManager.Domain.Entities;
using InsuranceManager.Domain.Ports;

namespace InsuranceManager.Application.Services;

public class PolicyService
{
    private readonly IPolicyRepository _policyRepository;
    private readonly IProposalRepository _proposalRepository;

    public PolicyService(IPolicyRepository policyRepository, IProposalRepository proposalRepository)
    {
        _policyRepository = policyRepository;
        _proposalRepository = proposalRepository;
    }

    public async Task<Policy> ContractAsync(ContractPolicyCommand command, CancellationToken ct = default)
    {
        var proposal = await _proposalRepository.GetByIdAsync(command.ProposalId, ct)
            ?? throw new InvalidOperationException("Proposal not found");

        if (!proposal.CanBeContracted())
            throw new InvalidOperationException("Only approved proposals can be contracted");

        var policy = Policy.CreateFromApprovedProposal(proposal);
        return await _policyRepository.AddAsync(policy, ct);
    }

    public async Task<Policy?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _policyRepository.GetByIdAsync(id, ct);

    public async Task<IReadOnlyList<Policy>> GetAllAsync(CancellationToken ct = default)
        => await _policyRepository.GetAllAsync(ct);
}