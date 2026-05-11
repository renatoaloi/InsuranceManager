using InsuranceManager.Application.Commands;
using InsuranceManager.Domain.Entities;
using InsuranceManager.Domain.Ports;
using InsuranceManager.Domain.ValueObjects;

namespace InsuranceManager.Application.Services;

public class ProposalService
{
    private readonly IProposalRepository _repository;
    private readonly IProposalReadAdapter _readAdapter;
    private readonly IHueyTaskRunner? _hueyTaskRunner;

    public ProposalService(IProposalRepository repository, IProposalReadAdapter readAdapter, IHueyTaskRunner? hueyTaskRunner = null)
    {
        _repository = repository;
        _readAdapter = readAdapter;
        _hueyTaskRunner = hueyTaskRunner;
    }

    public async Task<Proposal> CreateAsync(CreateProposalCommand command, CancellationToken ct = default)
    {
        var proposal = Proposal.Create(command.ClientName, command.CoverageType);
        return await _repository.AddAsync(proposal, ct);
    }

    public async Task<Proposal?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _repository.GetByIdAsync(id, ct);

    public async Task<IReadOnlyList<ProposalListItem>> GetAllAsync(
        ProposalStatus? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken ct = default)
    {
        return await _readAdapter.GetAllAsync(status, fromDate, toDate, ct);
    }

    public async Task EnqueueStatusChangeAsync(ChangeProposalStatusCommand command, CancellationToken ct = default)
    {
        var proposal = await _repository.GetByIdAsync(command.ProposalId, ct)
            ?? throw new InvalidOperationException("Proposal not found");

        if (!proposal.CanTransitionTo(command.NewStatus))
            throw new InvalidOperationException($"Invalid transition from {proposal.Status} to {command.NewStatus}");

        Console.WriteLine($"EnqueueStatusChangeAsync called with proposalId={command.ProposalId}, newStatus={command.NewStatus}");

        if (_hueyTaskRunner != null)
        {
            Console.WriteLine($"Calling HueyTaskRunner.EnqueueStatusChangeAsync");
            await _hueyTaskRunner.EnqueueStatusChangeAsync(command.ProposalId, command.NewStatus, ct);
            Console.WriteLine("HueyTaskRunner.EnqueueStatusChangeAsync completed");
        }
        else
        {
            Console.WriteLine("HueyTaskRunner is null, skipping enqueue");
            await Task.CompletedTask;
        }
    }

    public async Task<Proposal> ChangeStatusAsync(ChangeProposalStatusCommand command, CancellationToken ct = default)
    {
        var proposal = await _repository.GetByIdAsync(command.ProposalId, ct)
            ?? throw new InvalidOperationException("Proposal not found");

        if (!proposal.CanTransitionTo(command.NewStatus))
            throw new InvalidOperationException($"Invalid transition from {proposal.Status} to {command.NewStatus}");

        switch (command.NewStatus)
        {
            case ProposalStatus.Aprovada:
                proposal.Approve();
                break;
            case ProposalStatus.Recusada:
                proposal.Reject();
                break;
        }

        await _repository.UpdateAsync(proposal, ct);
        return proposal;
    }
}