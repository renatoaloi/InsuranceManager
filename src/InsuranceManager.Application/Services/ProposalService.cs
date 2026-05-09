using InsuranceManager.Application.Commands;
using InsuranceManager.Domain.Entities;
using InsuranceManager.Domain.Ports;
using InsuranceManager.Domain.ValueObjects;

namespace InsuranceManager.Application.Services;

public class ProposalService
{
    private readonly IProposalRepository _repository;

    public ProposalService(IProposalRepository repository) => _repository = repository;

    public async Task<Proposal> CreateAsync(CreateProposalCommand command, CancellationToken ct = default)
    {
        var proposal = Proposal.Create(command.ClientName, command.CoverageType);
        return await _repository.AddAsync(proposal, ct);
    }

    public async Task<Proposal?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _repository.GetByIdAsync(id, ct);

    public async Task<IReadOnlyList<Proposal>> GetAllAsync(ProposalStatus? status = null, CancellationToken ct = default)
        => await _repository.GetAllAsync(status, ct);
}