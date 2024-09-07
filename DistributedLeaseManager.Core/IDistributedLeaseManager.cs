namespace DistributedLeaseManager.Core;

public interface IDistributedLeaseManager
{
    Task<IDistributedLeaseAcquisitionResult> TryAcquireLease(string resourceId, TimeSpan duration);
    Task<IDistributedLeaseAcquisitionResult> TryAcquireLease(string resourceCategory, string resourceId, TimeSpan duration);
}
