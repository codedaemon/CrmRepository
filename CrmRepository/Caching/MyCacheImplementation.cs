using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using CrmRepository.EntityProviders;
using CrmRepository.Helpers;

namespace CrmRepository.Caching
{
    public class MyCacheImplementation :ICache, IEntityProvider
    {
        private Dictionary<Type, TypeCache> cache;
        private Dictionary<Type, SingleValueTypeCache> singleValueCache;

        public TimeSpan DefaultCacheDuration { get; set; }

        public MyCacheImplementation()
        {
            cache = new Dictionary<Type, TypeCache>();
            singleValueCache = new Dictionary<Type, SingleValueTypeCache>();
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
            //if this item exist in cache, then update it. If not, add it
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

        public bool SaveSingleValue<T>(string lookupProperty, object lookupValue, string propertyToCache, object valueToCache)
        {
            SaveSingleValueInternal<T>(lookupProperty, lookupValue, propertyToCache, valueToCache);
            SaveSingleValueInternal<T>(propertyToCache, valueToCache, lookupProperty, lookupValue);
            return true;
        }

        private void SaveSingleValueInternal<T>(string lookupProperty, object lookupValue, string propertyToCache, object valueToCache)
        {
            if (!singleValueCache.ContainsKey(typeof (T)))
            {
                singleValueCache.Add(typeof (T), new SingleValueTypeCache());
            }
            //if this item exist in cache, then update it. If not, add it
            var typeCache = singleValueCache[typeof (T)];
            var key = GenerateSingleValueKey(lookupProperty, lookupValue, propertyToCache);
            if (typeCache.CacheEntries.ContainsKey(key))
            {
                typeCache.CacheEntries[key] = new TypeCacheEntry(valueToCache, GenerateExpiryTime(FindShortestCacheDuration<T>(lookupProperty, propertyToCache)));
                Console.WriteLine("Updated SingleValueCache with id " + key + " inside the cache for " + typeof (T).Name);
            }
            else
            {
                typeCache.CacheEntries.Add(key, new TypeCacheEntry(valueToCache, GenerateExpiryTime(FindShortestCacheDuration<T>(lookupProperty, propertyToCache))));
                Console.WriteLine("Added SingleValueCache with id " + key + " inside the cache for " + typeof (T).Name);
            }
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

        public T GetInstance<T>(object key, IEnumerable<string> columns) where T : class
        {
            return GetInstance<T>(key);
        }

        public TResult GetValue<T, TResult>(object key, string propertyName) where T : class
        {
            var cacheKey = GenerateSingleValueKey(Constants.IdProperty, key, propertyName);
            return GetFromSingleValueCache<T, TResult>(cacheKey);
        }

        public TResult GetKey<T, TProperty, TResult>(string propertyName, TProperty propertyValue) where T : class
        {
            var cacheKey = GenerateSingleValueKey(propertyName, propertyValue, Constants.IdProperty);
            return GetFromSingleValueCache<T, TResult>(cacheKey);
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
                cache.Add(type, new TypeCache(duration));
            }
        }

        public void SetCacheDurationForSingleValueType<T>(Expression<Func<T, object>> property, TimeSpan duration)
        {
            var type = typeof(T);
            var key = ExpressionHelper.GetPropertyNameFromExpression(property);
            if (!singleValueCache.ContainsKey(type))
            {
                singleValueCache.Add(type, new SingleValueTypeCache());
            }
            var typeCache = singleValueCache[type];
            if (typeCache.PropertyCacheExpiry.ContainsKey(key))
            {
                typeCache.PropertyCacheExpiry[key] = duration;
            }
            else
            {
                typeCache.PropertyCacheExpiry.Add(key, duration);
            }
        }

        private TResult GetFromSingleValueCache<T, TResult>(string key) where T:class
        {
            if (!singleValueCache.ContainsKey(typeof(T)))
            {
                return default(TResult);
            }
            var typeCache = singleValueCache[typeof(T)];
            if (typeCache.CacheEntries.ContainsKey(key))
            {
                var cacheEntry = typeCache.CacheEntries[key];
                if (HasCacheExpired(cacheEntry))
                {
                    Console.WriteLine("Found entry with id " + key + " from the " + typeof(T).Name + " SingleValueCache, but the item has expired.");
                    typeCache.CacheEntries.Remove(key);
                    return default(TResult);
                }
                Console.WriteLine("Returned entry with id " + key + " from the " + typeof(T).Name + " SingleValueCache.");
                return (TResult)cacheEntry.Object;
            }
            return default(TResult);
        }

        private string GenerateSingleValueKey(string lookupProperty, object lookupValue, string cacheProperty)
        {
            return lookupProperty + "." + lookupValue + "=>" + cacheProperty;
        }

        private bool HasCacheExpired(TypeCacheEntry cacheEntry)
        {
            return DateTime.UtcNow > cacheEntry.CacheExpires;
        }

        private TimeSpan FindShortestCacheDuration<T>(string propertyName1, string propertyName2)
        {
            var typeCache = singleValueCache[typeof(T)];
            var duration1 = typeCache.PropertyCacheExpiry.ContainsKey(propertyName1) ? typeCache.PropertyCacheExpiry[propertyName1] : DefaultCacheDuration;
            var duration2 = typeCache.PropertyCacheExpiry.ContainsKey(propertyName2) ? typeCache.PropertyCacheExpiry[propertyName2] : DefaultCacheDuration;
            Console.WriteLine("Cache duration for single value calculated to be " + (duration1 < duration2 ? duration1 : duration2));
            return duration1 < duration2 ? duration1 : duration2;
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

        private class SingleValueTypeCache
        {
            public Dictionary<object, TypeCacheEntry> CacheEntries { get; set; }
            public Dictionary<string, TimeSpan> PropertyCacheExpiry { get; private set; }

            public SingleValueTypeCache()
            {
                CacheEntries = new Dictionary<object, TypeCacheEntry>();
                PropertyCacheExpiry = new Dictionary<string, TimeSpan>();
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