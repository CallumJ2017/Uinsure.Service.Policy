using Application.Services.SellPolicy;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class DependencyInjection
{
    public static void AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IPolicySalesService, PolicySalesService>();
    }
}
