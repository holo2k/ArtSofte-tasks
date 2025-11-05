using Core.DistributedSemaphore.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Core.DistributedSemaphore;

public static class RedisSemaphoreExtensions
{
    public static IServiceCollection AddRedisDistributedSemaphore(this IServiceCollection services,
        string redisConfiguration)
    {
        if (string.IsNullOrWhiteSpace(redisConfiguration))
            throw new ArgumentNullException(nameof(redisConfiguration));

        var mux = ConnectionMultiplexer.Connect(redisConfiguration);
        services.AddSingleton<IConnectionMultiplexer>(mux);
        services.AddSingleton<IDistributedSemaphore, RedisDistributedSemaphore>();

        return services;
    }
}