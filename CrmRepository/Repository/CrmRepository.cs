using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using CrmRepository.Caching;
using CrmRepository.EntityProviders;

namespace CrmRepository.Repository
{
    public class CrmRepository : ICrmRepository
    {
        public ICache Cache { get; set; }
        public List<IEntityProvider> EntityProviders { get; set; }

        private readonly Dictionary<Type, Type> _selectedTypeProviders;

        public CrmRepository()
        {
            EntityProviders = new List<IEntityProvider>();
            _selectedTypeProviders = new Dictionary<Type, Type>();
        }

        public void AddSelectedTypeProvider<T, T2>() where T2 : IEntityProvider
        {
            _selectedTypeProviders.Add(typeof (T), typeof (T2));
        }

        public T GetInstance<T>(object key) where T : class
        {
            foreach (var provider in GetEntityProvidersForType<T>())
            {
                object instance = provider.GetInstance<T>(key);
                if (instance != null)
                {
                    if (provider != Cache)
                    {
                        Cache.Save((T)instance);
                    }
                    return (T)instance;
                }
            }
            return null;
        }

        public T GetInstance<T>(object key, Func<T, object> columns) where T : class, new()
        {
            var columnnames = GetColumnList(columns);
            foreach (var provider in GetEntityProvidersForType<T>())
            {
                object instance = provider.GetInstance<T>(key, columnnames);
                if (instance != null)
                {
                    return (T)instance;
                }
            }
            return null;
        }

        public TResult GetValue<T, TResult>(object key, Expression<Func<T, TResult>> property) where T : class
        {
            var propertyName = GetPropertyNameFromExpression(property);
            foreach (var provider in GetEntityProvidersForType<T>())
            {
                TResult value = provider.GetValue<T, TResult>(key, propertyName);
                if (value != null && !value.Equals(default(TResult)))
                {
                    return value;
                }
            }
            return default(TResult);
        }

        public TResult GetKey<T, TProperty, TResult>(Expression<Func<T, TProperty>> property, TProperty propertyValue) where T : class
        {
            var propertyName = GetPropertyNameFromExpression(property);
            foreach (var provider in GetEntityProvidersForType<T>())
            {
                TResult value = provider.GetKey<T, TProperty, TResult>(propertyName, propertyValue);
                if (value != null && !value.Equals(default(TResult)))
                {
                    return value;
                }
            }
            return default(TResult);
        }

        private IEnumerable<IEntityProvider> GetEntityProvidersForType<T>() where T : class
        {
            var providers = EntityProviders;
            if (_selectedTypeProviders.ContainsKey(typeof (T)))
            {
                providers = providers.Where(p => p is ICache || p.GetType() == _selectedTypeProviders[typeof (T)]).ToList();
            }
            return providers;
        }


        //Helper methods
        private static string GetPropertyNameFromExpression<T, TResult>(Expression<Func<T, TResult>> property) where T : class
        {
            return (((MemberExpression) property.Body).Member as PropertyInfo).Name;
        }

        private static IList<string> GetColumnList<T>(Func<T, object> columns) where T : class, new()
        {
            return columns(new T()).GetType().GetProperties().Select(pi => pi.Name).ToList();
        }
    }
}