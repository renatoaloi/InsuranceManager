using Microsoft.AspNetCore.Mvc;
using InsuranceManager.Application.Commands;
using InsuranceManager.Application.Services;
using InsuranceManager.Api.DTOs;

namespace InsuranceManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PoliciesController : ControllerBase
{
    private readonly PolicyService _policyService;
    private readonly ProposalService _proposalService;

    public PoliciesController(PolicyService policyService, ProposalService proposalService)
    {
        _policyService = policyService;
        _proposalService = proposalService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PolicyResponseDto>>> GetAll(CancellationToken ct)
    {
        var policies = await _policyService.GetAllAsync(ct);
        return Ok(policies.Select(p => p.ToDto()).ToList());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PolicyResponseDto>> GetById(Guid id, CancellationToken ct)
    {
        var policy = await _policyService.GetByIdAsync(id, ct);
        if (policy is null)
            return NotFound();
        return Ok(policy.ToDto());
    }

    /// <summary>
    /// Contract an approved proposal to create a policy
    /// </summary>
    [HttpPost("/api/proposals/{proposalId:guid}/contract")]
    public async Task<ActionResult<PolicyResponseDto>> Contract(
        Guid proposalId,
        CancellationToken ct)
    {
        var proposal = await _proposalService.GetByIdAsync(proposalId, ct);
        if (proposal is null)
            return NotFound("Proposal not found");
        
        if (!proposal.CanBeContracted())
            return BadRequest("Only approved proposals can be contracted");

        var command = new ContractPolicyCommand(proposalId);
        var policy = await _policyService.ContractAsync(command, ct);
        return Created($"/api/policies/{policy.Id}", policy.ToDto());
    }
}