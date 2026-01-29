using Application.Abstractions.Services.Entities;
using Application.Abstractions.Services.Utils;
using Application.Services;
using Application.Services.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IClientService, ClientService>();
        services.AddScoped<ICompanyService, CompanyService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IStatusService, StatusService>();

        services.AddScoped<IIdentityService, IdentityService>();
        
        return services;
    }
}
