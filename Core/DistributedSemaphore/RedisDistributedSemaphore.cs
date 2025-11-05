using Core.DistributedSemaphore.Interfaces;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Core.DistributedSemaphore;

public class RedisDistributedSemaphore : IDistributedSemaphore
{
    private const string AcquireScript = @"
local key = KEYS[1]
local maxPermits = tonumber(ARGV[1])
local token = ARGV[2]
local now = tonumber(ARGV[3])
local ttl = tonumber(ARGV[4])

-- remove expired
redis.call('ZREMRANGEBYSCORE', key, '-inf', now - ttl)

local count = redis.call('ZCARD', key)
if count < maxPermits then
  redis.call('ZADD', key, now, token)
  -- set expire in ms (safety margin)
  redis.call('PEXPIRE', key, ttl + 1000)
  return 1
end
return 0
";

    private const string ReleaseScript = @"
local key = KEYS[1]
local token = ARGV[1]
return redis.call('ZREM', key, token)
";

    private readonly TimeSpan _acquirePollDelay = TimeSpan.FromMilliseconds(100);
    private readonly IDatabase _db;
    private readonly ILogger<RedisDistributedSemaphore> _log;
    private readonly IConnectionMultiplexer _redis;

    public RedisDistributedSemaphore(IConnectionMultiplexer redis, ILogger<RedisDistributedSemaphore> log)
    {
        _redis = redis ?? throw new ArgumentNullException(nameof(redis));
        _log = log ?? throw new ArgumentNullException(nameof(log));
        _db = _redis.GetDatabase();
    }

    public async Task<SemaphoreLease?> AcquireAsync(string key, int maxPermits, TimeSpan timeout, TimeSpan leaseTtl,
        int permits = 1, CancellationToken cancellationToken = default)
    {
        if (permits != 1)
            throw new NotSupportedException("Multi-permit acquire is not implemented in this version.");

        var stopAt = DateTime.UtcNow + timeout;
        var token = Guid.NewGuid().ToString("N");

        _log.LogDebug("AcquireAsync start key={Key} max={Max} timeout={Timeout} ttl={Ttl} token={Token}",
            key, maxPermits, timeout, leaseTtl, token);

        while (DateTime.UtcNow < stopAt)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var acquired = await TryAcquireInternal(key, maxPermits, token, leaseTtl, permits).ConfigureAwait(false);
            if (acquired)
            {
                _log.LogInformation("Acquire succeeded key={Key} token={Token}", key, token);
                return new SemaphoreLease(this, key, token, permits);
            }

            await Task.Delay(_acquirePollDelay, cancellationToken).ConfigureAwait(false);
        }

        _log.LogInformation("Acquire timeout key={Key}", key);
        return null;
    }

    public async Task<SemaphoreLease?> TryAcquireAsync(string key, int maxPermits, TimeSpan leaseTtl, int permits = 1,
        CancellationToken cancellationToken = default)
    {
        if (permits != 1)
            throw new NotSupportedException("Multi-permit acquire is not implemented in this version.");

        var token = Guid.NewGuid().ToString("N");
        var ok = await TryAcquireInternal(key, maxPermits, token, leaseTtl, permits).ConfigureAwait(false);
        if (ok)
        {
            _log.LogInformation("TryAcquire succeeded key={Key} token={Token}", key, token);
            return new SemaphoreLease(this, key, token, permits);
        }

        _log.LogDebug("TryAcquire failed key={Key}", key);
        return null;
    }

    public async Task ReleaseAsync(string key, string token, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(token)) return;

        try
        {
            var res = await _db.ScriptEvaluateAsync(ReleaseScript,
                new RedisKey[] { key },
                new RedisValue[] { token }).ConfigureAwait(false);

            long removed = 0;
            if (res.Type == ResultType.Integer) removed = (long)res;
            else if (res.Type == ResultType.SimpleString || res.Type == ResultType.BulkString)
                long.TryParse(res.ToString(), out removed);

            _log.LogInformation("Release key={Key} token={Token} removed={Removed}", key, token, removed);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Release failed key={Key} token={Token}", key, token);
        }
    }

    private async Task<bool> TryAcquireInternal(string key, int maxPermits, string token, TimeSpan leaseTtl,
        int permits)
    {
        if (permits != 1)
            throw new NotSupportedException("Multi-permit acquire is not implemented in this version.");

        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var ttl = (long)leaseTtl.TotalMilliseconds;

        try
        {
            var res = await _db.ScriptEvaluateAsync(AcquireScript,
                new RedisKey[] { key },
                new RedisValue[] { maxPermits, token, now, ttl }).ConfigureAwait(false);

            long r = 0;
            if (res.Type == ResultType.Integer) r = (long)res;
            else if (res.Type == ResultType.SimpleString || res.Type == ResultType.BulkString)
                long.TryParse(res.ToString(), out r);

            var success = r == 1;
            _log.LogDebug("TryAcquireInternal key={Key} token={Token} now={Now} ttl={Ttl} result={Result}",
                key, token, now, ttl, success);
            return success;
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "TryAcquireInternal failed key={Key} token={Token}", key, token);
            return false;
        }
    }
}