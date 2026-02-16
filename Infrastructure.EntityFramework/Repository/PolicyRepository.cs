using Domain.Aggregates;
using Domain.Repository;
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

    public async Task<Policy?> GetByIdAsync(Guid policyId)
    {
        return await _context.Policies.FirstOrDefaultAsync(x => x.Id == policyId);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}