using Domain.Aggregates;
using Domain.ValueObjects;

namespace Domain.Repository;

public interface IPolicyRepository
{
    Task Add(Policy policy);
    Task<Policy?> GetByReferenceAsync(PolicyReference policyReference);
    Task SaveChangesAsync();
}
