using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrmRepository.Caching;
using CrmRepository.Entities;
using CrmRepository.EntityProviders;
using CrmRepository.Repository;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace CrmRepository
{
    class Program
    {
        private static ICrmRepository Repository = null;

        static void Main(string[] args)
        {
            ApplicationStartup();
            
            //Fetch the store using a custom agent
            var storeId = Guid.Parse("4a5bdc79-207c-454f-b473-26a9382cd53c");
            var store = Repository.GetInstance<Store>(storeId);
            System.Threading.Thread.Sleep(5000);
            var store2 = Repository.GetInstance<Store>(storeId);

            //Fetch a customer using the mongodb provider
            var customerId = Guid.Parse("1a5bdc80-207c-454f-b473-26a9382cd53d");
            var customer = Repository.GetInstance<Customer>(customerId);
            var customer2 = Repository.GetInstance<Customer>(customerId);

            Console.WriteLine("Press [ENTER] to exit.");
            Console.ReadLine();
        }

        private static void ApplicationStartup()
        {
            var cacheImplementation = new MyCacheImplementation();
            //configure cache
            cacheImplementation.SetCacheDurationForType<Store>(TimeSpan.FromSeconds(3));

            var repository = new Repository.CrmRepository
            {
                Cache = cacheImplementation
            };

            //configure entity providers
            repository.EntityProviders.Add(cacheImplementation);
            repository.EntityProviders.Add(new AgentEntityProvider());
            repository.EntityProviders.Add(new MongoDbProvider());
            Repository = repository;
        }
    }
}
