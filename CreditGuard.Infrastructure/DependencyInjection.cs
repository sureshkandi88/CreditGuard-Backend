using CreditGuard.Core.Interfaces;
using CreditGuard.Infrastructure.Data;
using CreditGuard.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CreditGuard.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("DefaultConnection")));

        services.AddDbContext<BlobDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("BlobConnection")));

        services.AddMemoryCache();

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IWalletService, WalletService>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IGroupService, GroupService>();
        services.AddScoped<ICreditService, CreditService>();
        services.AddScoped<ICollectionService, CollectionService>();
        services.AddScoped<IBlobService, BlobService>();

        return services;
    }
}
