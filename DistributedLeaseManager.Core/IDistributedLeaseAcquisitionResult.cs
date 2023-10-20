namespace DistributedLeaseManager.Core;

public interface IDistributedLeaseAcquisitionResult : IAsyncDisposable
{
    bool IsSuccessful { get; }
}
