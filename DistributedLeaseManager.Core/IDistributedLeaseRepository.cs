namespace DistributedLeaseManager.Core;

public interface IDistributedLeaseRepository
{
    Task EnsureCreated();
    Task<DistributedLease?> Find(string resourceCategory, string resourceId);
    Task<bool> Add(DistributedLease lease);
    Task<bool> Update(DistributedLease lease);
    Task<bool> Remove(DistributedLease lease);
}
