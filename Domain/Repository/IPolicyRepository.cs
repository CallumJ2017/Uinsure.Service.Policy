using Domain.Aggregates;

namespace Domain.Repository;

public interface IPolicyRepository
{
    Task Add(Policy policy);
    Task<Policy?> GetByIdAsync(Guid policyId);
    Task SaveChangesAsync();
}
