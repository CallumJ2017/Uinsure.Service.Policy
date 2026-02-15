using SharedKernel;

namespace Domain.Entities;

public sealed class Policyholder : Entity<Guid>
{
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public DateOnly DateOfBirth { get; private set; }

    public Policyholder(string firstName, string lastName, DateOnly dateOfBirth) : base(Guid.NewGuid())
    {
        Guard.AgainstNullOrEmpty(firstName, "policy.invalid_name", "First name is required.");
        Guard.AgainstNullOrEmpty(lastName, "policy.invalid_name", "Last name is required.");
        Guard.AgainstDefault(dateOfBirth, "policy.invalid_dob", "Date of birth is required.");

        FirstName = firstName;
        LastName = lastName;
        DateOfBirth = dateOfBirth;
    }

    public static Policyholder Create(string firstName, string lastName, DateOnly dateOfBirth)
        => new(firstName, lastName, dateOfBirth);

    public int AgeAtPolicyStartDate(DateOnly policyStartDate)
    {
        var age = policyStartDate.Year - DateOfBirth.Year;

        // Adjust if birthday hasn’t occurred yet in that year
        if (DateOfBirth > policyStartDate.AddYears(-age))
            age--;

        return age;
    }
}
