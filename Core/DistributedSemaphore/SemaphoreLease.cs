using Core.DistributedSemaphore.Interfaces;

namespace Core.DistributedSemaphore;

public sealed class SemaphoreLease : IAsyncDisposable, IDisposable
{
    private readonly IDistributedSemaphore _semaphore;

    private int _released;

    public SemaphoreLease(IDistributedSemaphore semaphore, string key, string token, int permits = 1)
    {
        _semaphore = semaphore ?? throw new ArgumentNullException(nameof(semaphore));
        Key = key;
        Token = token;
        Permits = permits;
    }

    public string Key { get; }
    public string Token { get; }
    public int Permits { get; }

    public ValueTask DisposeAsync()
    {
        return new ValueTask(ReleaseAsync(CancellationToken.None));
    }

    public void Dispose()
    {
        ReleaseAsync(CancellationToken.None).GetAwaiter().GetResult();
    }

    public Task ReleaseAsync(CancellationToken cancellationToken = default)
    {
        return Interlocked.Exchange(ref _released, 1) == 1
            ? Task.CompletedTask
            : _semaphore.ReleaseAsync(Key, Token, cancellationToken);
    }
}