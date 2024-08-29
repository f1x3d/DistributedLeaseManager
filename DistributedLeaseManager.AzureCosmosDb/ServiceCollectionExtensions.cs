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
        string containerName,
        string partitionKeyPath)
    {
        services.TryAddSingleton(new CosmosClient(cosmosDbConnectionString));

        return services.AddCosmosDbDistributedLeaseManager(databaseName, containerName, partitionKeyPath);
    }

    public static IServiceCollection AddCosmosDbDistributedLeaseManager(
        this IServiceCollection services,
        string databaseName,
        string containerName,
        string partitionKeyPath)
    {
        if (!partitionKeyPath.StartsWith("/"))
        {
            throw new ArgumentException("Must start with /.", nameof(partitionKeyPath));
        }

        services.Configure<DistributedLeaseCosmosDbOptions>(options =>
        {
            options.DatabaseName = databaseName;
            options.ContainerName = containerName;
            options.PartitionKeyPath = partitionKeyPath;
        });

        services.TryAddScoped<IDistributedLeaseRepository, DistributedLeaseCosmosDb>();
        services.TryAddScoped<IDistributedLeaseManager, DistributedLeaseManager.Core.DistributedLeaseManager>();

        return services;
    }
}
