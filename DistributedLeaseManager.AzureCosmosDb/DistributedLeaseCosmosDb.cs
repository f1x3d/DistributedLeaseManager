using System.Net;
using DistributedLeaseManager.Core;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

namespace DistributedLeaseManager.AzureCosmosDb;

public class DistributedLeaseCosmosDb : IDistributedLeaseRepository
{
    private readonly CosmosClient _cosmosClient;
    private readonly DistributedLeaseCosmosDbOptions _options;

    public DistributedLeaseCosmosDb(
        CosmosClient cosmosClient,
        IOptions<DistributedLeaseCosmosDbOptions> options)
    {
        _cosmosClient = cosmosClient;
        _options = options.Value;
    }

    public async Task EnsureCreated()
    {
        await _cosmosClient.CreateDatabaseIfNotExistsAsync(_options.DatabaseName);

        await _cosmosClient
            .GetDatabase(_options.DatabaseName)
            .CreateContainerIfNotExistsAsync(_options.ContainerName, "/pk");
    }

    public async Task<bool> Add(DistributedLease lease)
    {
        var cosmosLease = new
        {
            id = lease.ResourceId.ToString(),
            category = lease.ResourceCategory,
            expirationTime = lease.ExpirationTime,
            pk = GetPartitionKey(lease),
        };

        try
        {
            await _cosmosClient
                .GetDatabase(_options.DatabaseName)
                .GetContainer(_options.ContainerName)
                .CreateItemAsync(cosmosLease, new(cosmosLease.pk));

            return true;
        }
        catch (CosmosException ex)
        when (ex.StatusCode == HttpStatusCode.Conflict)
        {
            return false;
        }
    }

    public async Task<DistributedLease?> Find(string resourceCategory, Guid resourceId)
    {
        try
        {
            var response = await _cosmosClient
                .GetDatabase(_options.DatabaseName)
                .GetContainer(_options.ContainerName)
                .ReadItemAsync<JObject>(resourceId.ToString(), new(GetPartitionKey(resourceCategory, resourceId)));

            return new()
            {
                ResourceId = response.Resource["id"].ToObject<Guid>(),
                ResourceCategory = response.Resource["category"].ToObject<string>(),
                ExpirationTime = response.Resource["expirationTime"].ToObject<DateTimeOffset>(),
                ETag = response.ETag,
            };
        }
        catch (CosmosException ex)
        when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<bool> Update(DistributedLease lease)
    {
        var cosmosLease = new
        {
            id = lease.ResourceId.ToString(),
            category = lease.ResourceCategory,
            expirationTime = lease.ExpirationTime,
            pk = GetPartitionKey(lease),
        };

        try
        {
            await _cosmosClient
                .GetDatabase(_options.DatabaseName)
                .GetContainer(_options.ContainerName)
                .ReplaceItemAsync(cosmosLease, cosmosLease.id, new(cosmosLease.pk), new()
                {
                    IfMatchEtag = lease.ETag
                });

            return true;
        }
        catch (CosmosException ex)
        when (ex.StatusCode == HttpStatusCode.PreconditionFailed)
        {
            return false;
        }
    }

    public async Task<bool> Remove(DistributedLease lease)
    {
        await _cosmosClient
            .GetDatabase(_options.DatabaseName)
            .GetContainer(_options.ContainerName)
            .DeleteItemAsync<JObject>(lease.ResourceId.ToString(), new(GetPartitionKey(lease)));

        return true;
    }

    private static string GetPartitionKey(DistributedLease lease)
        => GetPartitionKey(lease.ResourceCategory, lease.ResourceId);

    private static string GetPartitionKey(string resourceCategory, Guid resourceId)
        => $"{resourceCategory}/{resourceId}";
}
