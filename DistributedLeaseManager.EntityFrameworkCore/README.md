# DistributedLeaseManager.EntityFrameworkCore

[![NuGet](https://img.shields.io/nuget/v/DistributedLeaseManager.EntityFrameworkCore)](https://nuget.org/packages/DistributedLeaseManager.EntityFrameworkCore)

## Description

A simple C#/.NET [distributed lease/lock manager (DLM)](https://en.wikipedia.org/wiki/Distributed_lock_manager) implementation.

Inspired by the https://github.com/fbeltrao/azfunctions-distributed-locking

This library contains a lease storage implemented using the Entity Framework Core.

## Usage

1. Register the Distributed Lease Manager in the DI container using the EF Core database provider of your choice:
    ```csharp
    builder.Services.AddEfCoreDistributedLeaseManager(builder =>
        builder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Integrated Security=true;Database=DatabaseName",
            options => options.MigrationsAssembly(Assembly.GetExecutingAssembly().FullName)),
        "DistributedLeases");
    ```

1. Add and apply a [database migration](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations):
    ```bash
    dotnet ef migrations add InitDistributedLeasesTable
    dotnet ef database update
    ```
 
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
