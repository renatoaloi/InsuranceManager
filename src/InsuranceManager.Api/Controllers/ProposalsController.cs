using Microsoft.AspNetCore.Mvc;
using InsuranceManager.Application.Commands;
using InsuranceManager.Application.Services;
using InsuranceManager.Api.DTOs;
using InsuranceManager.Domain.ValueObjects;

namespace InsuranceManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProposalsController : ControllerBase
{
    private readonly ProposalService _proposalService;

    public ProposalsController(ProposalService proposalService) => _proposalService = proposalService;

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
    public async Task<ActionResult<IReadOnlyList<ProposalResponseDto>>> GetAll(
        [FromQuery] ProposalStatus? status,
        CancellationToken ct)
    {
        var proposals = await _proposalService.GetAllAsync(status, ct);
        return Ok(proposals.Select(p => p.ToDto()).ToList());
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