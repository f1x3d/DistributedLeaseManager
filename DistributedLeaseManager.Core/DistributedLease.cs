namespace DistributedLeaseManager.Core;

public class DistributedLease
{
    public const string DefaultResourceCategory = "Default";

    public string ResourceId { get; set; } = string.Empty;
    public string ResourceCategory { get; set; } = DefaultResourceCategory;
    public DateTimeOffset ExpirationTime { get; set; }
    public string ETag { get; set; } = string.Empty;
}
