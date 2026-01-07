using MAG.TOF.Application.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace MAG.TOF.Infrastructure.Services
{
    public class InMemoryCacheService : ICacheService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<InMemoryCacheService> _logger;

        public InMemoryCacheService(IMemoryCache memoryCache, ILogger<InMemoryCacheService> logger)
        {
            _memoryCache = memoryCache;
            _logger = logger;
        }

        // Gets a value from cache
        public Task<T?> GetAsync<T>(string key) where T : class
        {
            _logger.LogDebug("Getting item from cache with key: {Key}", key);

            // TryGetValue returns true if the key was found in cache
            if (_memoryCache.TryGetValue<T>(key, out T? value))
            {
                _logger.LogDebug("Cache HIT for key: {Key}", key);
                return Task.FromResult(value);
            }
            _logger.LogDebug("Cache MISS for key: {Key}", key);
            return Task.FromResult<T?>(null);
        }

        // Stores value in cache
        public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
        {
            _logger.LogDebug("Setting cache key: {Key} with expiration: {Expiration}", 
                key, expiration ?? TimeSpan.FromMinutes(30));

            var cacheOptions = new MemoryCacheEntryOptions
            {
                // Absolute expiration: Hard deadline
                AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(30),
                
                // Sliding Expiration: extends if accessed
                SlidingExpiration = TimeSpan.FromMinutes(10),

                // Priority: Eviction priority
                // Normal =  removed under memory pressure if needed
                Priority = CacheItemPriority.Normal
            };

            _memoryCache.Set(key, value, cacheOptions);

            _logger.LogInformation("Cache set for key: {Key}", key);
            return Task.CompletedTask;
        }

        public Task RemoveAsync(string key)
        {
            _logger.LogDebug("Removing cache key: {Key}", key);
            _memoryCache.Remove(key);
            return Task.CompletedTask;
        }

        // Gets from cache or creates using factory function
        public async Task<T> GetOrCreateAsync<T>(string key, 
            Func<Task<T>> factory, TimeSpan? expiration = null) where T : class
        {
            // Try to get from cache
            var cachedValue = await GetAsync<T>(key);

            if (cachedValue != null)
            {
                _logger.LogDebug("Returning cached value for key: {Key}", key);
                return cachedValue;
            }

            // Cache miss - create the value
            _logger.LogInformation("Cache miss for key : {Key}. Fetching from source...", key);

            var value = await factory(); // call the factory function

            // Store in cache for next time
            await SetAsync(key, value, expiration);
            return value;
        }




    }
}
