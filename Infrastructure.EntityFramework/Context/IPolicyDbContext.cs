using Domain.Aggregates;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.EntityFramework.Context;

public interface IPolicyDbContext
{
    DbSet<Policy> Policies { get; set; }
    DbSet<Policyholder> Policyholders { get; set; }
    DbSet<Payment> Payments { get; set; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
