using System.Text;
using DistributedLeaseManager.Core;

var builder = WebApplication.CreateBuilder(args);

// The following example uses a local Azurite instance
// See https://learn.microsoft.com/en-us/azure/storage/common/storage-use-azurite
builder.Services.AddDistributedLeaseManager("UseDevelopmentStorage=true", "distributed-leases");

var app = builder.Build();

app.MapGet("/distributed-leases", async (IDistributedLeaseManager leaseManager) =>
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
