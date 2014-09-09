using System;
using System.Data.Entity.Design.PluralizationServices;
using System.Globalization;
using System.Linq;
using MongoDB.Driver;

namespace CrmRepository.EntityProviders
{
    public class MongoDbProvider : IEntityProvider
    {
        private const string ConnectionString = "mongodb://localhost";
        private const string DatabaseName = "test";
        private MongoDatabase database;

        public MongoDbProvider()
        {

            var client = new MongoClient(ConnectionString);
            var server = client.GetServer();
            database = server.GetDatabase(DatabaseName);

        }

        public T GetInstance<T>(object key) where T:class
        {
            var pluralizationService = PluralizationService.CreateService(new CultureInfo("en-US"));
            var collectionName = pluralizationService.Pluralize(typeof (T).Name);

            var type = typeof (T);
            var keyProperty = type.GetProperty("Id");
            if (keyProperty == null)
            {
                keyProperty = type.GetProperty(type.Name + "Id");
                if (keyProperty == null)
                {
                    return null;
                }
            }

            var entries = database.GetCollection<T>(collectionName).FindAll().ToList();
            foreach (var entry in entries)
            {
                if (keyProperty.GetValue(entry).Equals(key))
                {
                    Console.WriteLine("Returning " + type.Name + " with Id " + key + " from MongoDbProvider");
                    return entry;
                }
            }
            return null;
        }
    }
}