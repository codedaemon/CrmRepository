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
            var storeId2 = Guid.Parse("5a5bdc79-307c-554f-c473-36a9382cd53c");
            //var store = Repository.GetInstance<Store>(storeId);
            //System.Threading.Thread.Sleep(1000);
            //var store2 = Repository.GetInstance<Store>(storeId);
            //var store3 = Repository.GetInstance<Store>(storeId);

            //Fetch a customer using the mongodb provider
            var customerId = Guid.Parse("1a5bdc80-207c-454f-b473-26a9382cd53d");
            //var customer = Repository.GetInstance<Customer>(customerId, c => new {c.Id, c.Name});
            //var customer2 = Repository.GetInstance<Customer>(customerId, c => new {c.Age});

            var customerName = Repository.GetValue<Customer, string>(customerId, c => c.Name);
            //var customerNameFromCache = Repository.GetValue<Customer, string>(customerId, c => c.Name);
            var customerKey = Repository.GetKey<Customer, string, Guid>(c => c.Name, customerName);
            var storeKey = Repository.GetKey<Store, string, Guid>(s => s.Name, "Testsjappa");

            //List<MyOwnActivityModelClass> activities = Repository.DoSomeSpecialMethodToGetLotsOfActivities(someinputparams);
            //List<Customer> customers = Repository.DoSomeSpecialMethodToGetCustomers(someinputparams);
            var customers = Repository.Get( () => StaticCustomerList("a"));

            var partialStoreFromCache = Repository.GetInstance<Store>(storeId, c => new {c.StoreId, c.Name});
            var partialStoreFromAgent = Repository.GetInstance<Store>(storeId2, c => new { c.StoreId, c.OpeningHours });

            Console.WriteLine("Press [ENTER] to exit.");
            Console.ReadLine();
        }

        private static List<Customer> StaticCustomerList(string nameMatch)
        {
            return new List<Customer>
            {
                new Customer
                {
                    Id = Guid.NewGuid(),
                    Name = "Harry",
                    Age = 23,
                    SecretAgentId = "001"
                },
                new Customer
                {
                    Id = Guid.NewGuid(),
                    Name = "Scott",
                    Age = 24,
                    SecretAgentId = "002"
                },
                new Customer
                {
                    Id = Guid.NewGuid(),
                    Name = "James",
                    Age = 25,
                    SecretAgentId = "007"
                }
            }.Where( c => c.Name.Contains(nameMatch)).ToList();
        }

        //all this should go in a configuration class somewhere
        private static void ApplicationStartup()
        {
            var cacheImplementation = new MyCacheImplementation();
            //configure cache
            cacheImplementation.SetCacheDurationForType<Store>(TimeSpan.FromMilliseconds(500));
            cacheImplementation.SetCacheDurationForSingleValueType<Customer>(c => c.Name, TimeSpan.FromDays(1));
            cacheImplementation.SetCacheDurationForSingleValueType<Customer>(c => c.Id, TimeSpan.FromHours(2));

            var repository = new Repository.CrmRepository
            {
                Cache = cacheImplementation
            };
            //configure entity providers
            repository.EntityProviders.Add(cacheImplementation);
            repository.EntityProviders.Add(new AgentEntityProvider());
            repository.EntityProviders.Add(new MongoDbProvider());

            //configure specific entity providers per type
            repository.AddSelectedTypeProvider<Customer, MongoDbProvider>();

            Repository = repository;
        }
    }
}
