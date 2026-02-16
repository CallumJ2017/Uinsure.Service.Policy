using Domain.Aggregates;
using Domain.Repository;
using Domain.ValueObjects;
using Infrastructure.EntityFramework.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.EntityFramework.Repository;

public class PolicyRepository : IPolicyRepository
{
    private readonly IPolicyDbContext _context;

    public PolicyRepository(IPolicyDbContext context)
    {
        _context = context;
    }

    public async Task Add(Policy policy)
    {
        _context.Policies.Add(policy);
    }

    public async Task<Policy?> GetByReferenceAsync(PolicyReference policyReference)
    {
        return await _context.Policies
            .Include(x => x.InsuredProperty)
            .Include(x => x.PolicyHolders)
            .Include(x => x.Payments)
            .FirstOrDefaultAsync(x => x.Reference == policyReference);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
