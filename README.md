# DistributedLeaseManager

## NuGet packages

[![NuGet](https://img.shields.io/nuget/v/DistributedLeaseManager.Core?logo=nuget&label=DistributedLeaseManager.Core)](https://nuget.org/packages/DistributedLeaseManager.Core)\
[![NuGet](https://img.shields.io/nuget/v/DistributedLeaseManager.AzureBlobStorage?logo=nuget&label=DistributedLeaseManager.AzureBlobStorage)](https://nuget.org/packages/DistributedLeaseManager.AzureBlobStorage)\
[![NuGet](https://img.shields.io/nuget/v/DistributedLeaseManager.AzureCosmosDb?logo=nuget&label=DistributedLeaseManager.AzureCosmosDb)](https://nuget.org/packages/DistributedLeaseManager.AzureCosmosDb)\
[![NuGet](https://img.shields.io/nuget/v/DistributedLeaseManager.EntityFrameworkCore?logo=nuget&label=DistributedLeaseManager.EntityFrameworkCore)](https://nuget.org/packages/DistributedLeaseManager.EntityFrameworkCore)

## Description

A simple C#/.NET [distributed lease/lock manager (DLM)](https://en.wikipedia.org/wiki/Distributed_lock_manager) implementation.

Inspired by the https://github.com/fbeltrao/azfunctions-distributed-locking

## Usage

1. Install one of the lease storage implementations by following their README instructions:
	* [Azure Blob Storage](./DistributedLeaseManager.AzureBlobStorage)
	* [Azure Cosmos DB](./DistributedLeaseManager.AzureCosmosDb)
	* [Entity Framework Core](./DistributedLeaseManager.EntityFrameworkCore) (any database provider supported by the EF Core)

1. Inside your controller/service inject the `IDistributedLeaseManager` and call the `TryAcquireLease` method. Verify if the result was successful - if it was then you can proceed with the operation; otherwise, someone else has acquired the lease:
    ```csharp
    await using var leaseResult = await leaseManager.TryAcquireLease(resourceId, TimeSpan.FromSeconds(5));

    if (!leaseResult.IsSuccessful)
    {
        // Someone else has required the lease for the resource.
        // You may want to either retry the acqusition or abort the operation.
    }
    else
    {
        // You are the lease owner now and can safely process the resource.
        // The lease will be released either when the leaseResult gets disposed
        // or when the lease expires (in the example above, in 5 seconds)
    }
    ```

## Contributing / Implementing a custom lease storage

If you'd like to use a lease storage different from the ones provided by the author, add the `DistributedLeaseManager.Core` library to your project and implement the corresponding interface (see any of the existing implementations as an example).

Feel free to open a PR with your changes to include them in the package!
