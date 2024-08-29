using DistributedLeaseManager.AzureCosmosDb;
using DistributedLeaseManager.Core;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCosmosDbDistributedLeaseManager(
        this IServiceCollection services,
        string cosmosDbConnectionString,
        string databaseName,
        string containerName)
    {
        services.TryAddSingleton(new CosmosClient(cosmosDbConnectionString));

        return services.AddCosmosDbDistributedLeaseManager(databaseName, containerName);
    }

    public static IServiceCollection AddCosmosDbDistributedLeaseManager(
        this IServiceCollection services,
        string databaseName,
        string containerName)
    {
        return services.AddCosmosDbDistributedLeaseManager(options =>
        {
            options.DatabaseName = databaseName;
            options.ContainerName = containerName;
        });
    }

    public static IServiceCollection AddCosmosDbDistributedLeaseManager(
        this IServiceCollection services,
        string cosmosDbConnectionString,
        Action<DistributedLeaseCosmosDbOptions> optionsConfiguration)
    {
        services.TryAddSingleton(new CosmosClient(cosmosDbConnectionString));

        return services.AddCosmosDbDistributedLeaseManager(optionsConfiguration);
    }

    public static IServiceCollection AddCosmosDbDistributedLeaseManager(
        this IServiceCollection services,
        Action<DistributedLeaseCosmosDbOptions> optionsConfiguration)
    {
        services.Configure(optionsConfiguration);

        services.TryAddScoped<IDistributedLeaseRepository, DistributedLeaseCosmosDb>();
        services.TryAddScoped<IDistributedLeaseManager, DistributedLeaseManager.Core.DistributedLeaseManager>();

        return services;
    }
}
