using InsuranceManager.Domain.ValueObjects;

namespace InsuranceManager.Application.Commands;

public record CreateProposalCommand(string ClientName, CoverageType CoverageType);