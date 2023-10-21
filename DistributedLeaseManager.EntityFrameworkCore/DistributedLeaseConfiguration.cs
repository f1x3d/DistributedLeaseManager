using DistributedLeaseManager.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DistributedLeaseManager.EntityFrameworkCore;

public class DistributedLeaseConfiguration : IEntityTypeConfiguration<DistributedLease>
{
    private readonly string _tableName;

    public DistributedLeaseConfiguration(string tableName)
    {
        _tableName = tableName;
    }

    public void Configure(EntityTypeBuilder<DistributedLease> builder)
    {
        builder
            .ToTable(_tableName);

        builder
            .Property<Guid>("Id");

        builder
            .HasKey("Id");

        builder
            .Property(x => x.ResourceCategory)
            .HasMaxLength(255);

        builder
            .Property(x => x.ETag)
            .HasMaxLength(36);

        builder
            .HasIndex(x => new { x.ResourceCategory, x.ResourceId })
            .IsUnique();

        builder
            .Property(x => x.ETag)
            .IsConcurrencyToken();
    }
}
