using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CrmRepository.Agents;
using CrmRepository.Caching;
using CrmRepository.EntityProviders;

namespace CrmRepository.Repository
{
    public class CrmRepository : ICrmRepository
    {
        public ICache Cache { get; set; }
        public List<IEntityProvider> EntityProviders { get; set; }

        public CrmRepository()
        {
            EntityProviders = new List<IEntityProvider>();
        }



        public T GetInstance<T>(object key) where T : class
        {
            foreach (var provider in EntityProviders)
            {
                object instance = provider.GetInstance<T>(key);
                if (instance != null)
                {
                    if (provider != Cache)
                    {
                        Cache.Save((T) instance);
                    }
                    return (T) instance;
                }
            }
            return null;
        }

    }
}