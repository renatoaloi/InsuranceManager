using InsuranceManager.Domain.ValueObjects;

namespace InsuranceManager.Api.DTOs;

public record CreateProposalDto(string ClientName, CoverageType CoverageType);