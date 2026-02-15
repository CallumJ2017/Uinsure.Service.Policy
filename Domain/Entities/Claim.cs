using SharedKernel;

namespace Domain.Entities;

public sealed class Claim : Entity<Guid>
{
    public DateOnly DateOfIncident { get; private set; }

    internal Claim(Guid id, DateOnly dateOfIncident) : base(id)
    {
        DateOfIncident = dateOfIncident;
    }
}
