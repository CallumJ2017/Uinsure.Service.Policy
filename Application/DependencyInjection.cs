using Application.Services.CancelPolicy;
using Application.Services.GetPolicy;
using Application.Services.RenewPolicy;
using Application.Services.SellPolicy;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class DependencyInjection
{
    public static void AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IPolicySalesService, PolicySalesService>();
        services.AddScoped<IPolicyCancellationService, PolicyCancellationService>();
        services.AddScoped<IPolicyRenewalService, PolicyRenewalService>();
        services.AddScoped<IPolicyRetrievalService, PolicyRetrievalService>();
    }
}
