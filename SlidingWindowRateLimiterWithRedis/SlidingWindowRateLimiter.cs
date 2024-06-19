using StackExchange.Redis;
using System;
using System.Collections.Generic;

public class SlidingWindowRateLimiter
{
    private readonly ConnectionMultiplexer redis;
    private readonly IDatabase db;
    private readonly HashSet<string> whitelist;

    public SlidingWindowRateLimiter(string redisConnectionString)
    {
        try
        {
            redis = ConnectionMultiplexer.Connect(redisConnectionString + ",abortConnect=false");
            db = redis.GetDatabase();
        }
        catch (RedisConnectionException ex)
        {
            Console.WriteLine($"Redis connection failed: {ex.Message}");
            throw;
        }

        whitelist = new HashSet<string>();
    }

    public void AddToWhitelist(string ipAddress)
    {
        whitelist.Add(ipAddress);
    }

    public void RemoveFromWhitelist(string ipAddress)
    {
        whitelist.Remove(ipAddress);
    }

    public bool IsWhitelisted(string ipAddress)
    {
        return whitelist.Contains(ipAddress);
    }

    private string RateLimitKey(string ipAddress, string rateType)
    {
        return $"rate_limiter:{ipAddress}:{rateType}";
    }

    public bool IsRateLimited(string ipAddress, string rateType, int limit, TimeSpan windowSize)
    {
        if (IsWhitelisted(ipAddress))
        {
            return false;
        }

        string key = RateLimitKey(ipAddress, rateType);
        long currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        // Remove outdated entries
        db.SortedSetRemoveRangeByScore(key, double.NegativeInfinity, currentTime - windowSize.TotalMilliseconds);

        // Get the current request count
        long currentCount = db.SortedSetLength(key);

        return currentCount >= limit;
    }

    public void IncrementRequestCount(string ipAddress, string rateType, TimeSpan windowSize)
    {
        if (IsWhitelisted(ipAddress))
        {
            return;
        }

        string key = RateLimitKey(ipAddress, rateType);
        long currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        // Add the current request timestamp
        db.SortedSetAdd(key, currentTime.ToString(), currentTime);

        // Set the TTL (time-to-live) for the key based on the window size
        db.KeyExpire(key, windowSize);
    }
}
