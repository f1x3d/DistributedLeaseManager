using DistributedLeaseManager.Core;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System.Net;

namespace DistributedLeaseManager.AzureCosmosDb;

public class DistributedLeaseCosmosDb : IDistributedLeaseRepository
{
    private readonly CosmosClient _cosmosClient;
    private readonly DistributedLeaseCosmosDbOptions _options;
    private readonly string _partitionKeyPropertyName;

    public DistributedLeaseCosmosDb(
        CosmosClient cosmosClient,
        IOptions<DistributedLeaseCosmosDbOptions> options)
    {
        _cosmosClient = cosmosClient;
        _options = options.Value;
        _partitionKeyPropertyName = _options.PartitionKeyPath[1..];
    }

    public async Task EnsureCreated()
    {
        await _cosmosClient.CreateDatabaseIfNotExistsAsync(_options.DatabaseName);

        await _cosmosClient
            .GetDatabase(_options.DatabaseName)
            .CreateContainerIfNotExistsAsync(_options.ContainerName, _options.PartitionKeyPath);
    }

    public async Task<bool> Add(DistributedLease lease)
    {
        var cosmosLease = CreateLease(lease);

        try
        {
            await _cosmosClient
                .GetDatabase(_options.DatabaseName)
                .GetContainer(_options.ContainerName)
                .CreateItemAsync(cosmosLease, new (GetPartitionKey(lease)));

            return true;
        }
        catch (CosmosException ex)
        when (ex.StatusCode == HttpStatusCode.Conflict)
        {
            return false;
        }
    }

    public async Task<DistributedLease?> Find(string resourceCategory, string resourceId)
    {
        try
        {
            var response = await _cosmosClient
                .GetDatabase(_options.DatabaseName)
                .GetContainer(_options.ContainerName)
                .ReadItemAsync<JObject>(resourceId, new(GetPartitionKey(resourceCategory, resourceId)));

            return new()
            {
                ResourceId = response.Resource["id"].ToObject<string>(),
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
        var cosmosLease = CreateLease(lease);

        try
        {
            await _cosmosClient
                .GetDatabase(_options.DatabaseName)
                .GetContainer(_options.ContainerName)
                .ReplaceItemAsync(cosmosLease, cosmosLease["id"].ToString(), new(GetPartitionKey(lease)), new()
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
            .DeleteItemAsync<JObject>(lease.ResourceId, new(GetPartitionKey(lease)));

        return true;
    }

    private static string GetPartitionKey(DistributedLease lease)
        => GetPartitionKey(lease.ResourceCategory, lease.ResourceId);

    private static string GetPartitionKey(string resourceCategory, string resourceId)
        => $"{resourceCategory}/{resourceId}";

    private JObject CreateLease(DistributedLease lease) =>
        CreateLease(lease.ResourceId, lease.ResourceCategory, lease.ExpirationTime, GetPartitionKey(lease));

    private JObject CreateLease(string resourceId, string category, DateTimeOffset expirationTime, string partitionKey)
    {
        return new()
        {
            ["id"] = resourceId,
            ["category"] = category,
            ["expirationTime"] = expirationTime,
            [_partitionKeyPropertyName] = partitionKey,
        };
    }
}
