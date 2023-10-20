using Azure.Storage.Blobs;
using DistributedLeaseManager.AzureBlobStorage;
using DistributedLeaseManager.Core;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBlobStorageDistributedLeaseManager(
        this IServiceCollection services,
        string blobStorageConnectionString,
        string containerName)
    {
        services.TryAddSingleton(new BlobServiceClient(blobStorageConnectionString));

        return services.AddBlobStorageDistributedLeaseManager(containerName);
    }

    public static IServiceCollection AddBlobStorageDistributedLeaseManager(
        this IServiceCollection services,
        string containerName)
    {
        services.Configure<DistributedLeaseBlobStorageOptions>(options =>
            options.ContainerName = containerName);

        services.TryAddScoped<IDistributedLeaseRepository, DistributedLeaseBlobStorage>();
        services.TryAddScoped<IDistributedLeaseManager, DistributedLeaseManager.Core.DistributedLeaseManager>();

        return services;
    }
}
