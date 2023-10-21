namespace DistributedLeaseManager.AzureCosmosDb;

public class DistributedLeaseCosmosDbOptions
{
    public string DatabaseName { get; set; } = string.Empty;
    public string ContainerName { get; set; } = string.Empty;
}
