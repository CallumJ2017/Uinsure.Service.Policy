using Application.Services.GetPolicy;
using Application.Services.SellPolicy;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class DependencyInjection
{
    public static void AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IPolicySalesService, PolicySalesService>();
        services.AddScoped<IPolicyRetrievalService, PolicyRetrievalService>();
    }
}
