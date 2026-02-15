using Domain.Entities;
using Domain.Enums;
using Domain.ValueObjects;
using SharedKernel;

namespace Domain.Aggregates;

public sealed class Policy : AggregateRoot<Guid>
{
    private const int MaxAdvanceSaleDays = 60;
    private const int PolicyLengthInYears = 1;
    private const int MinimumNumberOfPolicyholders = 1;
    private const int MaximumNumberOfPolicyholders = 3;
    private const int MinimumPolicyholderAge = 16;

    private static DateOnly Today => DateOnly.FromDateTime(DateTime.UtcNow);

    public string Reference { get; private set; }
    public HomeInsuranceType InsuranceType { get; private set; }
    public Property InsuredProperty { get; private set; }
    public DateOnly StartDate { get; private set; }
    public DateOnly EndDate { get; private set; }
    public Money Premium { get; private set; }
    public bool AutoRenew { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset LastModifiedAt { get; private set; }


    private readonly List<Policyholder> _policyholders = new();
    public IReadOnlyCollection<Policyholder> PolicyHolders => _policyholders.AsReadOnly();


    private readonly List<Payment> _payments = new();
    public IReadOnlyCollection<Payment> Payments => _payments.AsReadOnly();


    private readonly List<Claim> _claims = new();
    public IReadOnlyCollection<Claim> Claims => _claims.AsReadOnly();


    public bool HasClaims => _claims.Count > 0;

    internal Policy(
        string reference,
        DateOnly startDate,
        Money amount,
        Property property,
        bool autoRenew,
        IEnumerable<Policyholder> policyHolders) : base(id: Guid.NewGuid())
    {
        Guard.AgainstNullOrEmpty(reference, "policy.invalid_reference", "Policy reference is required.");
        Guard.AgainstDefault(startDate, "policy.invalid_start_date", "Start date is required.");
        Guard.AgainstNull(amount, "policy.invalid_amount", "Policy amount is required.");
        Guard.AgainstNull(property, "policy.invalid_property", "Policy must have property to insure.");
        Guard.AgainstNull(policyHolders, "policy.invalid_policyholders", "Policy must have policyholders.");

        Reference = reference;
        StartDate = startDate;
        EndDate = startDate.AddYears(PolicyLengthInYears);
        Premium = amount;
        InsuredProperty = property;
        AutoRenew = autoRenew;
        CreatedAt = DateTimeOffset.UtcNow;

        _policyholders.AddRange(policyHolders);
    }

    /// <summary>
    /// Factory method to enforce business rules before creating the policy.
    /// </summary>
    public static Result<Policy> CreateNew(
        string reference,
        DateOnly startDate,
        Money premium,
        Property property,
        bool autoRenew,
        IEnumerable<Policyholder> policyHolders)
    {
        if (startDate > Today.AddDays(MaxAdvanceSaleDays))
            return Result<Policy>.Fail("policy.start.toofar", $"A policy can only be sold up to {MaxAdvanceSaleDays} days in advance.");

        var numberOfPolicyHolders = policyHolders?.Count() ?? 0;

        if (numberOfPolicyHolders < MinimumNumberOfPolicyholders || numberOfPolicyHolders > MaximumNumberOfPolicyholders)
            return Result<Policy>.Fail("policy.policyholders.count", $"A policy must have at least {MinimumNumberOfPolicyholders} policyholder but no more than {MaximumNumberOfPolicyholders}.");

        if (policyHolders!.Any(x => x.AgeAtPolicyStartDate(startDate) < MinimumPolicyholderAge))
            return Result<Policy>.Fail("policy.policyholders.minimum_age", $"All policyholders must meet the minimum age requirement of: {MinimumPolicyholderAge} at the start date of the policy.");

        return Result<Policy>.Success(new Policy(reference, startDate, premium, property, autoRenew, policyHolders!));
    }
}