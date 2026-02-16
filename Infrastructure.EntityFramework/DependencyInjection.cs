using Domain.Repository;
using Infrastructure.EntityFramework.Context;
using Infrastructure.EntityFramework.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.EntityFramework;

public static class DependencyInjection
{
    public static void AddEntityFramworkInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("PolicyDatabase");

        services.AddDbContext<PolicyDbContext>(options => options.UseSqlServer(connectionString));

        services.AddScoped<IPolicyDbContext>(provider => provider.GetRequiredService<PolicyDbContext>());
        services.AddScoped<IPolicyRepository, PolicyRepository>();
    }
}