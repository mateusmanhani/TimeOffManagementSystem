using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAG.TOF.Application.Interfaces
{
    public interface ICacheService
    {
        // GetAsync<T>(string key) -> T?

        // SetAsync<T>(string key, T value, TimeSpan? expiration = null)

        // RemoveAsync(string key) -> Task<bool> or Result

        // GetOrCreateAsync<T>(string key, Func<Task<T>> createItem, TimeSpan? expiration = null) -> T


    }
}
