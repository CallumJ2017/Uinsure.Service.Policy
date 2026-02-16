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

    public PolicyReference Reference { get; private set; }
    public HomeInsuranceType InsuranceType { get; private set; }
    public PolicyStatus Status { get; private set; }
    public Property InsuredProperty { get; private set; }
    public DateOnly StartDate { get; private set; }
    public DateOnly EndDate { get; private set; }
    public Money Premium { get; private set; }
    public bool AutoRenew { get; private set; }
    public bool HasClaims { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset LastModifiedAt { get; private set; }


    private readonly List<Policyholder> _policyholders = new();
    public IReadOnlyCollection<Policyholder> PolicyHolders => _policyholders;

    private readonly List<Payment> _payments = new();
    public IReadOnlyCollection<Payment> Payments => _payments;

    // Required by EF Core
    private Policy() { }

    internal Policy(
        HomeInsuranceType insuranceType,
        PolicyReference policyReference,
        DateOnly startDate,
        Money amount,
        Property property,
        bool autoRenew,
        bool hasClaims) : base(id: Guid.NewGuid())
    {
        Guard.AgainstNull(policyReference, "policy.invalid_reference", "Policy reference is required.");
        Guard.AgainstDefault(startDate, "policy.invalid_start_date", "Start date is required.");
        Guard.AgainstNull(amount, "policy.invalid_amount", "Policy amount is required.");

        InsuranceType = insuranceType;
        Reference = policyReference;
        StartDate = startDate;
        EndDate = startDate.AddYears(PolicyLengthInYears);
        Premium = amount;
        InsuredProperty = property;
        AutoRenew = autoRenew;
        HasClaims = hasClaims;
        Status = PolicyStatus.Draft;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public static Result<Policy> CreateNew(
        HomeInsuranceType type,
        DateOnly startDate,
        Money premium,
        string addressLine1,
        string postcode,
        bool autoRenew)
    {
        if (startDate > Today.AddDays(MaxAdvanceSaleDays))
            return Result<Policy>.Fail("policy.start.toofar", $"A policy can only be sold up to {MaxAdvanceSaleDays} days in advance.");

        var propertyResult = Property.Create(addressLine1, postcode);
        if (!propertyResult.IsSuccess)
            return Result<Policy>.Fail(propertyResult.Error.Code, propertyResult.Error.Message);

        var policyReference = PolicyReference.Generate(type);

        return Result<Policy>.Success(new Policy(type, policyReference, startDate, premium, propertyResult.Value!, autoRenew, false));
    }

    public Result Purchase()
    {
        if (Status != PolicyStatus.Draft)
            return Result.Fail("policy.invalid_state", "Only draft policies can be purchased.");

        if (_policyholders.Count < MinimumNumberOfPolicyholders)
            return Result.Fail("policy.policyholders.required", $"At least {MinimumNumberOfPolicyholders} policyholder is required.");

        if (InsuredProperty is null)
            return Result.Fail("policy.property.required", "An insured property is required before purchasing.");

        if (_payments.Count == 0)
            return Result.Fail("policy.payment.required", "A payment is required to purchase..");

        Status = PolicyStatus.Active;
        LastModifiedAt = DateTimeOffset.UtcNow;

        return Result.Success();
    }

    public Result<Policyholder> AddPolicyHolder(string firstName, string lastName, DateOnly dateOfBirth)
    {
        if (Status != PolicyStatus.Draft)
            return Result<Policyholder>.Fail("policy.locked", "Policyholders can only be added while policy is in Draft.");

        var policyholderResult = Policyholder.Create(firstName, lastName, dateOfBirth);

        if (!policyholderResult.IsSuccess)
            return policyholderResult;

        var policyholder = policyholderResult.Value!;

        if (_policyholders.Count + 1 > MaximumNumberOfPolicyholders)
            return Result<Policyholder>.Fail("policy.policyholders.max_count", $"Cannot have more than {MaximumNumberOfPolicyholders} policyholders.");

        if (policyholder.AgeAtPolicyStartDate(StartDate) < MinimumPolicyholderAge)
            return Result<Policyholder>.Fail("policy.policyholders.minimum_age", $"Policyholder must be at least {MinimumPolicyholderAge} years old at the policy start date.");

        _policyholders.Add(policyholder);

        return Result<Policyholder>.Success(policyholder);
    }

    public Result<Payment> AddPayment(string reference, PaymentMethod type, decimal amount)
    {
        if (Status != PolicyStatus.Draft)
            return Result<Payment>.Fail("policy.locked", "Payments can only be added while policy is in Draft.");

        var paymentResult = Payment.Create(reference, type, amount);
        if (!paymentResult.IsSuccess)
            return paymentResult;

        var payment = paymentResult.Value!;
        _payments.Add(payment);

        return Result<Payment>.Success(payment);
    }
}
