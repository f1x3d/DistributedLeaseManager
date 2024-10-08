﻿# DistributedLeaseManager.AzureCosmosDb

[![NuGet](https://img.shields.io/nuget/v/DistributedLeaseManager.AzureCosmosDb)](https://nuget.org/packages/DistributedLeaseManager.AzureCosmosDb)

## Description

A simple C#/.NET [distributed lease/lock manager (DLM)](https://en.wikipedia.org/wiki/Distributed_lock_manager) implementation.

Inspired by the https://github.com/fbeltrao/azfunctions-distributed-locking

This library contains a lease storage implemented using the Azure Cosmos DB.

## Usage

1. (Optionally) [Register your Azure Cosmos DB Client in the DI container](https://learn.microsoft.com/en-us/dotnet/azure/sdk/dependency-injection?tabs=web-app-builder#register-clients-and-subclients)

1. Register the Distributed Lease Manager in the DI container:
    ```csharp
    builder.Services.AddCosmosDbDistributedLeaseManager("DatabaseName", "DistributedLeases");
    ```

    or specify the Cosmos DB connection string in case you skipped the first step:
    ```csharp
    builder.Services.AddCosmosDbDistributedLeaseManager("ConnectionString", "DatabaseName", "DistributedLeases");
    ```

    or utilize one of the overloads that accepts an action in order to also customize the partition key path:
    ```csharp
    builder.Services.AddCosmosDbDistributedLeaseManager(
        "AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==",
        options =>
        {
            options.DatabaseName = "DatabaseName";
            options.ContainerName = "DistributedLeases";
            options.PartitionKeyPath = "/partitionKey";
        }); 
    ```
   Note: Partition Key Path must start with `/`. Nested partition key paths such as `/my/partitionKey` are not supported.
 
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
