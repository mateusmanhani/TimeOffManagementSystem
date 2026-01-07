using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAG.TOF.Application.Interfaces
{
    public interface ICacheService
    {
        // Gets a value from cache by key and returns null if not found
        Task<T?> GetAsync<T>(string key) where T : class;

        // Stores a value in cache with an optional expiration time 
        Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class;

        // Removes value from cache by key
        Task RemoveAsync(string key);

        // Gets value from cache, or creates it using the factory function if not found.
        Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null) where T : class;


    }
}
