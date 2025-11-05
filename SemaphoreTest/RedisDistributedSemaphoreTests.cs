using Core.DistributedSemaphore;
using Core.DistributedSemaphore.Interfaces;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using Xunit.Abstractions;

namespace SemaphoreTest;

public class RedisDistributedSemaphoreTests
{
    private readonly ITestOutputHelper _output;

    public RedisDistributedSemaphoreTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact(DisplayName = "Semaphore limits concurrent access")]
    public async Task Semaphore_Should_Limit_Concurrency()
    {
        // Arrange
        var loggerFactory = LoggerFactory.Create(b => b.SetMinimumLevel(LogLevel.Information));
        var redis = await ConnectionMultiplexer.ConnectAsync("localhost:6379");
        var logger = loggerFactory.CreateLogger<RedisDistributedSemaphore>();

        IDistributedSemaphore semaphore = new RedisDistributedSemaphore(redis, logger);

        var key = "test:semaphore:xunit";
        var maxPermits = 3;
        var totalTasks = 10;
        var leaseTtl = TimeSpan.FromSeconds(5);
        var timeout = TimeSpan.FromSeconds(2);

        var activeCount = 0;
        var maxActiveObserved = 0;
        var lockObj = new object();

        // Act
        var tasks = Enumerable.Range(1, totalTasks).Select(async i =>
        {
            await Task.Delay(i * 50); // немного смещаем старт

            var lease = await semaphore.AcquireAsync(key, maxPermits, timeout, leaseTtl);
            if (lease == null)
            {
                _output.WriteLine($"[{i}] ❌ Could not acquire semaphore");
                return;
            }

            var current = Interlocked.Increment(ref activeCount);
            lock (lockObj)
            {
                maxActiveObserved = Math.Max(maxActiveObserved, current);
            }

            _output.WriteLine($"[{i}] ✅ ENTER (active={current})");

            await Task.Delay(1000); // симуляция работы

            current = Interlocked.Decrement(ref activeCount);
            _output.WriteLine($"[{i}] 🏁 LEAVE (active={current})");

            await lease.ReleaseAsync();
        });

        await Task.WhenAll(tasks);

        // Assert
        _output.WriteLine($"Max concurrent active = {maxActiveObserved}");
        Assert.True(maxActiveObserved <= maxPermits,
            $"Semaphore allowed {maxActiveObserved} concurrent tasks (expected ≤ {maxPermits})");
    }
}