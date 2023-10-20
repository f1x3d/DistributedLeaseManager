namespace DistributedLeaseManager.Core;

public interface IDistributedLeaseManager
{
    Task<IDistributedLeaseAcquisitionResult> TryAcquireLease(Guid resourceId, TimeSpan duration);
    Task<IDistributedLeaseAcquisitionResult> TryAcquireLease(string resourceCategory, Guid resourceId, TimeSpan duration);
}
