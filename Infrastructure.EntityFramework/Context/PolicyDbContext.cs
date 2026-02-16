using Domain.Aggregates;
using Domain.Entities;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Infrastructure.EntityFramework.Context;

public class PolicyDbContext : DbContext, IPolicyDbContext
{
    public DbSet<Policy> Policies { get; set; }
    public DbSet<Policyholder> Policyholders { get; set; }
    public DbSet<Property> Properties { get; set; }
    public DbSet<Payment> Payments { get; set; }

    public PolicyDbContext(DbContextOptions<PolicyDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Global configuration for DateOnly
        var dateOnlyConverter = new ValueConverter<DateOnly, DateTime>(
            d => d.ToDateTime(TimeOnly.MinValue),
            d => DateOnly.FromDateTime(d));

        modelBuilder.Entity<Policy>(policy =>
        {
            policy.ToTable("Policies");
            policy.HasKey(p => p.Id);

            policy.Property(p => p.Reference)
                  .HasConversion(
                        v => v.Value,
                        v => PolicyReference.FromString(v))
                  .HasColumnName("Reference")
                  .HasMaxLength(20)
                  .IsRequired();

            policy.HasIndex(p => p.Reference).IsUnique();

            policy.Property(p => p.StartDate)
                  .HasConversion(dateOnlyConverter)
                  .HasColumnType("date")
                  .IsRequired();

            policy.Property(p => p.EndDate)
                  .HasConversion(dateOnlyConverter)
                  .HasColumnType("date")
                  .IsRequired();

            policy.Property(p => p.AutoRenew).IsRequired();
            policy.Property(p => p.HasClaims).IsRequired();

            policy.Property(p => p.CreatedAt)
                  .HasColumnType("datetimeoffset")
                  .IsRequired();

            policy.Property(p => p.LastModifiedAt)
                  .HasColumnType("datetimeoffset");

            policy.OwnsOne(p => p.Premium, money =>
            {
                money.WithOwner();

                money.Property(m => m.Value)
                     .HasColumnName("Premium")
                     .IsRequired();

                money.Ignore(m => m.Currency);
            });

            // Property Navigation (1:1)
            policy.HasOne(p => p.InsuredProperty)
                  .WithOne()
                  .HasForeignKey<Property>("PolicyId")
                  .OnDelete(DeleteBehavior.Cascade);

            // PolicyHolders Navigation (1:n)
            policy.HasMany(p => p.PolicyHolders)
                  .WithOne()
                  .HasForeignKey("PolicyId")
                  .IsRequired();

            // Payments Navigation (1:n)
            policy.HasMany(p => p.Payments)
                  .WithOne()
                  .HasForeignKey("PolicyId")
                  .IsRequired()
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Property Entity
        modelBuilder.Entity<Property>(prop =>
        {
            prop.ToTable("Properties");
            prop.HasKey(p => p.Id);

            prop.Property(p => p.AddressLine1).IsRequired();
            prop.Property(p => p.AddressLine2);
            prop.Property(p => p.AddressLine3);
            prop.Property(p => p.Postcode)
                .IsRequired()
                .HasMaxLength(8);
        });

        // Policyholder Entity
        modelBuilder.Entity<Policyholder>(entity =>
        {
            entity.ToTable("Policyholders");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.FirstName).IsRequired();
            entity.Property(e => e.LastName).IsRequired();

            // Map DateOfBirth as date if using DateOnly
            if (entity.Property(e => e.DateOfBirth).Metadata.ClrType == typeof(DateOnly))
            {
                entity.Property(e => e.DateOfBirth)
                      .HasConversion(dateOnlyConverter)
                      .HasColumnType("date")
                      .IsRequired();
            }
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.ToTable("Payments");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Reference)
                  .IsRequired()
                  .HasMaxLength(50);

            entity.Property(e => e.Type)
                  .IsRequired();

            entity.Property(e => e.Amount)
                  .HasColumnType("decimal(18,2)")
                  .IsRequired();
        });
    }
}
