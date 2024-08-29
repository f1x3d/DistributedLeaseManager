namespace DistributedLeaseManager.AzureCosmosDb;

public class DistributedLeaseCosmosDbOptions
{
    public string DatabaseName { get; set; } = string.Empty;
    public string ContainerName { get; set; } = string.Empty;

    private string _partitionKeyPath = "/pk";

    public string PartitionKeyPath
    {
        get => _partitionKeyPath;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (!value.StartsWith("/"))
            {
                throw new ArgumentException("Must start with /.", nameof(value));
            }

            if (value.LastIndexOf('/') > 0)
            {
                throw new ArgumentException("Nested partition key paths are not supported at this time.",
                    nameof(value));
            }

            _partitionKeyPath = value;
        }
    }
}
