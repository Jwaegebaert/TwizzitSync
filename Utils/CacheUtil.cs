using System;
using System.Runtime.Caching;

namespace TwizzitSync.Utils;

internal class CacheUtil
{
    private static readonly MemoryCache Cache = MemoryCache.Default;

    /// <summary>
    /// Try to get item from cache.
    /// </summary>
    /// <typeparam name="T">Item type.</typeparam>
    /// <param name="key">Key name.</param>
    /// <param name="item">Cached item.</param>
    /// <returns>True if item was found, else false.</returns>
    public static bool TryGetItem<T>(string key, out T item)
    {

        if (Cache.Contains(key))
        {
            item = (T)Cache.Get(key);
            return true;
        }

        item = default;
        return false;
    }

    /// <summary>
    /// Add item to the cache.
    /// </summary>
    /// <param name="key">Key name.</param>
    /// <param name="item">Item to cache.</param>
    /// <param name="expireIn">Expiration time.</param>
    public static void AddItem(string key, object item, TimeSpan expireIn)
    {
        var expiration = DateTimeOffset.Now.Add(expireIn);
        Cache.Add(key, item, expiration);
    }
}