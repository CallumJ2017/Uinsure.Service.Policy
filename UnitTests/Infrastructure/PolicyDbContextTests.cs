using Domain.Aggregates;
using Domain.Enums;
using Domain.ValueObjects;
using FluentAssertions;
using Infrastructure.EntityFramework.Context;
using Microsoft.EntityFrameworkCore;

namespace UnitTests.Infrastructure;

public class PolicyDbContextTests
{
    [Fact]
    public async Task SaveChanges_ShouldPersistPolicyPayments()
    {
        var databaseName = Guid.NewGuid().ToString();
        var options = new DbContextOptionsBuilder<PolicyDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        var startDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(10);
        var createResult = Policy.CreateNew(
            HomeInsuranceType.Household,
            startDate,
            Money.Create(300m),
            "1 Test Street",
            "AB12CD",
            autoRenew: true);

        createResult.IsSuccess.Should().BeTrue();
        var policy = createResult.Value!;
        policy.AddPayment("PAY-DB-001", PaymentMethod.Card, 123.45m).IsSuccess.Should().BeTrue();

        await using (var writeContext = new PolicyDbContext(options))
        {
            writeContext.Policies.Add(policy);
            await writeContext.SaveChangesAsync();
        }

        await using var readContext = new PolicyDbContext(options);
        var persistedPolicy = await readContext.Policies
            .Include(p => p.Payments)
            .SingleAsync();

        persistedPolicy.Payments.Should().HaveCount(1);
        var payment = persistedPolicy.Payments.Single();
        payment.Reference.Should().Be("PAY-DB-001");
        payment.Type.Should().Be(PaymentMethod.Card);
        payment.Amount.Should().Be(123.45m);
    }
}
