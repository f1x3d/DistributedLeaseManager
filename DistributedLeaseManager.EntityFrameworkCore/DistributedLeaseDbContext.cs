using DistributedLeaseManager.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DistributedLeaseManager.EntityFrameworkCore;

public class DistributedLeaseDbContext : DbContext
{
    private readonly DistributedLeaseEfCoreOptions _options;

    public DbSet<DistributedLease> Leases => Set<DistributedLease>();

    public DistributedLeaseDbContext(
        DbContextOptions dbContextOptions,
        IOptions<DistributedLeaseEfCoreOptions> distributedLeaseOptions)
        : base(dbContextOptions)
    {
        _options = distributedLeaseOptions.Value;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new DistributedLeaseConfiguration(_options.TableName));
    }
}
