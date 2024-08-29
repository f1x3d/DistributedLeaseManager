using System.Reflection;
using System.Text;
using DistributedLeaseManager.Core;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// The following example uses a local Azurite instance
// See https://learn.microsoft.com/en-us/azure/storage/common/storage-use-azurite
//builder.Services.AddDistributedLeaseManager("UseDevelopmentStorage=true", "distributed-leases");

// The following example uses a local Azure Cosmos DB emulator
// See https://learn.microsoft.com/en-us/azure/cosmos-db/emulator
//builder.Services.AddCosmosDbDistributedLeaseManager(
//    "AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==",
//    options =>
//    {
//        options.DatabaseName = "DatabaseName";
//        options.ContainerName = "DistributedLeases";
//        options.PartitionKeyPath = "/partitionKey";
//    });

// The following example uses a SQL Server Express LocalDB
// See https://learn.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb
builder.Services.AddEfCoreDistributedLeaseManager(builder =>
    builder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Integrated Security=true;Database=DatabaseName",
        options => options.MigrationsAssembly(Assembly.GetExecutingAssembly().FullName)),
    "DistributedLeases");

var app = builder.Build();

app.MapGet("/distributed-leases", async (IServiceProvider serviceProvider) =>
{
    const int parallelGroupSize = 5;
    var resourceId = Guid.NewGuid();
    var result = new StringBuilder();
    var lockObject = new object();

    // Try processing same resource in parallel

    await Task.WhenAll(Enumerable.Range(1, parallelGroupSize)
        .Select(i => ProcessResource(resourceId, i)));

    result.AppendLine("---");

    // Try processing the same resource in parallel
    // after the previous processor has finished gracefully

    await Task.WhenAll(Enumerable.Range(1, parallelGroupSize)
        .Select(i => ProcessResource(resourceId, i)));

    result.AppendLine("---");

    // Try processing the same resource in parallel
    // after the processor has failed to free the lease

    var stuckProcessor = ProcessResource(resourceId, 1, simulateFail: true);

    await Task.Delay(TimeSpan.FromSeconds(7));

    await Task.WhenAll(Enumerable.Range(2, parallelGroupSize)
        .Select(i => ProcessResource(resourceId, i)));

    return result.ToString();

    async Task ProcessResource(Guid resourceId, int invocationNumber, bool simulateFail = false)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var leaseManager = scope.ServiceProvider.GetRequiredService<IDistributedLeaseManager>();
        await using var leaseResult = await leaseManager.TryAcquireLease(resourceId, TimeSpan.FromSeconds(5));

        if (!leaseResult.IsSuccessful)
        {
            lock (lockObject)
            {
                result
                    .Append("Method invocation ")
                    .Append(invocationNumber)
                    .AppendLine(" could not acquire a lease.");
            }
        }
        else
        {
            lock (lockObject)
            {
                result
                    .Append("Method invocation ")
                    .Append(invocationNumber)
                    .AppendLine(" acquired a lease.");
            }

            // Simulate processing

            if (simulateFail)
                await Task.Delay(Timeout.InfiniteTimeSpan);
            else
                await Task.Delay(TimeSpan.FromSeconds(1));
        }
    }
});

app.Run();
