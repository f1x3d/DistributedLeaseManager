using System.Net;
using System.Text.Json;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using DistributedLeaseManager.Core;
using Microsoft.Extensions.Options;

namespace DistributedLeaseManager.AzureBlobStorage;

public class DistributedLeaseBlobStorage : IDistributedLeaseRepository
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly DistributedLeaseBlobStorageOptions _options;

    public DistributedLeaseBlobStorage(
        BlobServiceClient blobServiceClient,
        IOptions<DistributedLeaseBlobStorageOptions> options)
    {
        _blobServiceClient = blobServiceClient;
        _options = options.Value;
    }

    public Task EnsureCreated()
        => _blobServiceClient
            .GetBlobContainerClient(_options.ContainerName)
            .CreateIfNotExistsAsync();

    public async Task<bool> Add(DistributedLease lease)
    {
        using var jsonStream = new MemoryStream();
        JsonSerializer.Serialize(jsonStream, lease);
        jsonStream.Position = 0;

        try
        {
            await _blobServiceClient
                .GetBlobContainerClient(_options.ContainerName)
                .UploadBlobAsync(GetBlobPath(lease), jsonStream);

            return true;
        }
        catch (RequestFailedException ex)
        when (ex.Status == (int)HttpStatusCode.Conflict)
        {
            return false;
        }
    }

    public async Task<DistributedLease?> Find(string resourceCategory, string resourceId)
    {
        using var blobStream = new MemoryStream();

        try
        {
            var response = await _blobServiceClient
                .GetBlobContainerClient(_options.ContainerName)
                .GetBlobClient(GetBlobPath(resourceCategory, resourceId))
                .DownloadToAsync(blobStream);
            blobStream.Position = 0;

            var lease = JsonSerializer.Deserialize<DistributedLease>(blobStream)!;
            lease.ETag = response.Headers.ETag.ToString()!;

            return lease;
        }
        catch (RequestFailedException ex)
        when (ex.Status == (int)HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<bool> Update(DistributedLease lease)
    {
        using var jsonStream = new MemoryStream();
        JsonSerializer.Serialize(jsonStream, lease);
        jsonStream.Position = 0;

        try
        {
            await _blobServiceClient
                .GetBlobContainerClient(_options.ContainerName)
                .GetBlobClient(GetBlobPath(lease))
                .UploadAsync(jsonStream, new BlobUploadOptions
                {
                    Conditions = new BlobRequestConditions
                    {
                        IfMatch = new ETag(lease.ETag)
                    }
                });

            return true;
        }
        catch (RequestFailedException ex)
        when (ex.Status == (int)HttpStatusCode.PreconditionFailed)
        {
            return false;
        }
    }

    public async Task<bool> Remove(DistributedLease lease)
    {
        var response = await _blobServiceClient
            .GetBlobContainerClient(_options.ContainerName)
            .DeleteBlobIfExistsAsync(GetBlobPath(lease));

        return response.Value;
    }

    private static string GetBlobPath(DistributedLease lease)
        => GetBlobPath(lease.ResourceCategory, lease.ResourceId);

    private static string GetBlobPath(string resourceCategory, string resourceId)
        => $"{resourceCategory}/{resourceId}.json";
}
