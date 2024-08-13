using DAIKIN.CheckSheetPortal.Infrastructure.Services;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAIKIN.CheckSheetPortal.Services
{
    public class CacheService : ICacheService
    {
        private readonly IMemoryCache _cache;
        public CacheService(IMemoryCache cache)
        {
            _cache = cache;
        }
        public T Get<T>(string key)
        {
            return _cache.Get<T>(key);
        }
        public void Set<T>(string key, T value, TimeSpan expirationTime)
        {
            _cache.Set(key, value, expirationTime);
        }
        public void Remove(string key)
        {
            _cache.Remove(key);
        }
    }
}
