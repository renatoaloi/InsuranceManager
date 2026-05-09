using InsuranceManager.Domain.Entities;

namespace InsuranceManager.Domain.Ports;

public interface IPolicyRepository
{
    Task<Policy?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Policy>> GetAllAsync(CancellationToken ct = default);
    Task<Policy> AddAsync(Policy policy, CancellationToken ct = default);
}