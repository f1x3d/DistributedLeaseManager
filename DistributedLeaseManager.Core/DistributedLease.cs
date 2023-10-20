namespace DistributedLeaseManager.Core;

public class DistributedLease
{
    public const string DefaultResourceCategory = "Default";

    public Guid ResourceId { get; set; }
    public string ResourceCategory { get; set; } = DefaultResourceCategory;
    public DateTimeOffset ExpirationTime { get; set; }
    public string ETag { get; set; } = string.Empty;
}
