using DistributedLeaseManager.Core;
using Microsoft.EntityFrameworkCore;

namespace DistributedLeaseManager.EntityFrameworkCore;

public class DistributedLeaseEfCore : IDistributedLeaseRepository
{
    private readonly DistributedLeaseDbContext _dbContext;

    public DistributedLeaseEfCore(DistributedLeaseDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task EnsureCreated()
        => _dbContext.Database.EnsureCreatedAsync();

    public async Task<bool> Add(DistributedLease lease)
    {
        try
        {
            lease.ETag = Guid.NewGuid().ToString();
            _dbContext.Add(lease);
            await _dbContext.SaveChangesAsync();

            return true;
        }
        catch (DbUpdateException)
        {
            return false;
        }
    }

    public Task<DistributedLease?> Find(string resourceCategory, Guid resourceId)
        => _dbContext.Leases.FirstOrDefaultAsync(x =>
            x.ResourceCategory == resourceCategory &&
            x.ResourceId == resourceId);

    public async Task<bool> Update(DistributedLease lease)
    {
        try
        {
            lease.ETag = Guid.NewGuid().ToString();
            _dbContext.Update(lease);
            await _dbContext.SaveChangesAsync();

            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            return false;
        }
    }

    public async Task<bool> Remove(DistributedLease lease)
    {
        _dbContext.Remove(lease);
        await _dbContext.SaveChangesAsync();

        return true;
    }
}
