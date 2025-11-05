namespace Core.DistributedSemaphore.Interfaces;

public interface IDistributedSemaphore
{
    Task<SemaphoreLease?> AcquireAsync(string key, int maxPermits, TimeSpan timeout, TimeSpan leaseTtl, int permits = 1,
        CancellationToken cancellationToken = default);

    Task<SemaphoreLease?> TryAcquireAsync(string key, int maxPermits, TimeSpan leaseTtl, int permits = 1,
        CancellationToken cancellationToken = default);

    Task ReleaseAsync(string key, string token, CancellationToken cancellationToken = default);
}