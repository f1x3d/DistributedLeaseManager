using DistributedLeaseManager.Core;
using DistributedLeaseManager.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEfCoreDistributedLeaseManager(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder> dbContextConfiguration,
        string tableName)
    {
        services.AddDbContext<DistributedLeaseDbContext>(dbContextConfiguration);

        return services.AddEfCoreDistributedLeaseManager<DistributedLeaseDbContext>(tableName);
    }

    private static IServiceCollection AddEfCoreDistributedLeaseManager<T>(
        this IServiceCollection services,
        string tableName)
        where T : DbContext
    {
        services.Configure<DistributedLeaseEfCoreOptions>(options =>
            options.TableName = tableName);

        services.TryAddScoped<IDistributedLeaseRepository, DistributedLeaseEfCore>();
        services.TryAddScoped<IDistributedLeaseManager, DistributedLeaseManager.Core.DistributedLeaseManager>();

        return services;
    }
}
