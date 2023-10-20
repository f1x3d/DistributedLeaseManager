namespace DistributedLeaseManager.Core;

public class DistributedLeaseAcquisitionResult : IDistributedLeaseAcquisitionResult
{
    private static readonly DistributedLeaseAcquisitionResult FailureResult = new();

    private readonly IDistributedLeaseRepository? _repository;
    private readonly DistributedLease? _lease;

    public bool IsSuccessful { get; }

    protected DistributedLeaseAcquisitionResult()
    {
    }

    protected DistributedLeaseAcquisitionResult(IDistributedLeaseRepository repository, DistributedLease lease)
    {
        IsSuccessful = true;

        _repository = repository
            ?? throw new ArgumentNullException(nameof(repository));

        _lease = lease
            ?? throw new ArgumentNullException(nameof(lease));
    }

    public static DistributedLeaseAcquisitionResult Success(IDistributedLeaseRepository repository, DistributedLease lease)
        => new(repository, lease);

    public static DistributedLeaseAcquisitionResult Failure()
        => FailureResult;

    public ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);

        return IsSuccessful
            ? new(_repository!.Remove(_lease!))
            : ValueTask.CompletedTask;
    }
}
