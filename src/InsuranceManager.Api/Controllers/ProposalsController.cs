using Microsoft.AspNetCore.Mvc;
using InsuranceManager.Application.Commands;
using InsuranceManager.Application.Services;
using InsuranceManager.Api.DTOs;
using InsuranceManager.Domain.Ports;
using InsuranceManager.Domain.ValueObjects;

namespace InsuranceManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProposalsController : ControllerBase
{
    private readonly ProposalService _proposalService;
    private readonly IProposalReadAdapter _readAdapter;

    public ProposalsController(ProposalService proposalService, IProposalReadAdapter readAdapter)
    {
        _proposalService = proposalService;
        _readAdapter = readAdapter;
    }

    [HttpPost]
    public async Task<ActionResult<ProposalResponseDto>> Create(
        [FromBody] CreateProposalDto dto,
        CancellationToken ct)
    {
        var command = new CreateProposalCommand(dto.ClientName, dto.CoverageType);
        var proposal = await _proposalService.CreateAsync(command, ct);
        return CreatedAtAction(nameof(GetById), new { id = proposal.Id }, proposal.ToDto());
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProposalResponseDto>>> GetAll(
        [FromQuery] ProposalStatus? status = null,
        CancellationToken ct = default)
    {
        var proposals = await _readAdapter.GetAllAsync(status, ct: ct);
        return Ok(proposals.Select(p => new ProposalResponseDto(p.Id, p.ClientName, p.CoverageType, p.Status, p.CreatedAt, p.UpdatedAt)));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProposalResponseDto>> GetById(
        Guid id,
        CancellationToken ct)
    {
        var proposal = await _proposalService.GetByIdAsync(id, ct);
        if (proposal is null)
            return NotFound();
        return Ok(proposal.ToDto());
    }
}