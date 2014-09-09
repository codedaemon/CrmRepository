using System;
using System.Collections.Generic;
using CrmRepository.EntityProviders;

namespace CrmRepository.Caching
{
    public class MyCacheImplementation :ICache, IEntityProvider
    {
        private Dictionary<Type, TypeCache> cache;

        public TimeSpan DefaultCacheDuration { get; set; }

        public MyCacheImplementation()
        {
            cache = new Dictionary<Type, TypeCache>();
            DefaultCacheDuration = TimeSpan.FromMinutes(10);
        }
        public bool Save<T>(T instance)
        {
            var key = GetKeyFromInstance(instance);
            if (key == null)
            {
                return false;
            }
            if (!cache.ContainsKey(typeof (T)))
            {
                cache.Add(typeof(T), new TypeCache(DefaultCacheDuration));
            }
            //if this item exist in cache, then update it. If not, delete it
            var typeCache = cache[typeof (T)];
            if (typeCache.CacheEntries.ContainsKey(key))
            {
                Console.WriteLine("Updated entry with id " + key + " inside the cache for " + typeof (T).Name);
                typeCache.CacheEntries[key] = new TypeCacheEntry(instance, GenerateExpiryTime(typeCache.CacheDuration));
            }
            else
            {
                Console.WriteLine("Added entry with id " + key + " inside the cache for " + typeof(T).Name);
                typeCache.CacheEntries.Add(key, new TypeCacheEntry(instance, GenerateExpiryTime(typeCache.CacheDuration)));
            }
            return true;
        }

        public T GetInstance<T>(object key) where T : class
        {
            if (!cache.ContainsKey(typeof (T)))
            {
                return null;
            }
            var typeCache = cache[typeof (T)];
            if (typeCache.CacheEntries.ContainsKey(key))
            {
                var cacheEntry = typeCache.CacheEntries[key];
                if (HasCacheExpired(cacheEntry))
                {
                    Console.WriteLine("Found entry with id " + key + " from the " + typeof(T).Name + " cache, but the item has expired.");
                    typeCache.CacheEntries.Remove(key);
                    return null;
                }
                Console.WriteLine("Returned entry with id " + key + " from the " + typeof (T).Name + " cache.");
                return (T)cacheEntry.Object;
            }
            return null;
        }

        public void SetCacheDurationForType<T>(TimeSpan duration)
        {
            var type = typeof (T);
            if (cache.ContainsKey(type))
            {
                cache[type].CacheDuration = duration;
            }
            else
            {
                cache.Add(typeof(T), new TypeCache(duration));
            }
        }

        private bool HasCacheExpired(TypeCacheEntry cacheEntry)
        {
            return DateTime.UtcNow > cacheEntry.CacheExpires;
        }

        private DateTime GenerateExpiryTime(TimeSpan cacheDuration)
        {
            return DateTime.UtcNow + cacheDuration;
        }

        private object GetKeyFromInstance(object instance)
        {
            var type = instance.GetType();
            //first, we check if the instance has a property called Id
            var propertyinfo = type.GetProperty("Id");
            if (propertyinfo != null)
            {
                return propertyinfo.GetValue(instance);
            }
            //then, we check if the instance has a property called <instancetype>Id
            propertyinfo = type.GetProperty(type.Name + "Id");
            if (propertyinfo != null)
            {
                return propertyinfo.GetValue(instance);
            }
            //then, we need some way of configuring what is the correct property for types that does not match
            //if all else fails, we return null
            return null;
        }

        private class TypeCache
        {
            public Dictionary<object,TypeCacheEntry> CacheEntries { get; set; }
            public TimeSpan CacheDuration { get; set; }

            public TypeCache(TimeSpan duration)
            {
                CacheDuration = duration;
                CacheEntries = new Dictionary<object, TypeCacheEntry>();
            }
        }

        private class TypeCacheEntry
        {
            public object Object { get; set; }
            public DateTime CacheExpires { get; set; }

            public TypeCacheEntry(object obj, DateTime expires)
            {
                Object = obj;
                CacheExpires = expires;
            }
        }
    }


}