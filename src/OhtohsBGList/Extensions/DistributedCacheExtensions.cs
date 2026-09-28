using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace OhtohsBGList.Extensions;

/// <summary>
/// Extension methods that give <see cref="IDistributedCache"/> a JSON-backed,
/// strongly-typed API similar to <see cref="Microsoft.Extensions.Caching.Memory.IMemoryCache"/>.
/// </summary>
public static class DistributedCacheExtensions
{
    /// <summary>
    /// Attempts to read and JSON-deserialize a cached value.
    /// </summary>
    public static bool TryGetValue<T>(this IDistributedCache cache, string key, [NotNullWhen(true)] out T? value)
    {
        var bytes = cache.Get(key);
        if (bytes is null)
        {
            value = default;
            return false;
        }

        value = JsonSerializer.Deserialize<T>(bytes);
        return value is not null;
    }

    /// <summary>
    /// JSON-serializes and stores a value with an absolute expiration relative to now.
    /// </summary>
    public static void Set<T>(this IDistributedCache cache, string key, T value, TimeSpan absoluteExpirationRelativeToNow)
    {
        cache.Set(
            key,
            JsonSerializer.SerializeToUtf8Bytes(value),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow
            });
    }
}
