namespace DistributedLeaseManager.Core;

public class DistributedLeaseManager : IDistributedLeaseManager
{
    private readonly IDistributedLeaseRepository _repository;

    public DistributedLeaseManager(IDistributedLeaseRepository repository)
    {
        _repository = repository
            ?? throw new ArgumentNullException(nameof(repository));
    }

    public Task<IDistributedLeaseAcquisitionResult> TryAcquireLease(Guid resourceId, TimeSpan duration)
        => TryAcquireLease(DistributedLease.DefaultResourceCategory, resourceId, duration);

    public async Task<IDistributedLeaseAcquisitionResult> TryAcquireLease(
        string resourceCategory,
        Guid resourceId,
        TimeSpan duration)
    {
        await _repository.EnsureCreated();

        var existingLease = await _repository.Find(resourceCategory, resourceId);

        if (existingLease is not null && DateTime.UtcNow < existingLease.ExpirationTime)
            return DistributedLeaseAcquisitionResult.Failure();

        var leaseRecord = existingLease ?? new()
        {
            ResourceCategory = resourceCategory,
            ResourceId = resourceId,
        };

        leaseRecord.ExpirationTime = DateTime.UtcNow + duration;

        if (existingLease is not null && await _repository.Update(existingLease))
            return DistributedLeaseAcquisitionResult.Success(_repository, existingLease);

        if (await _repository.Add(leaseRecord))
            return DistributedLeaseAcquisitionResult.Success(_repository, leaseRecord);

        return DistributedLeaseAcquisitionResult.Failure();
    }
}
