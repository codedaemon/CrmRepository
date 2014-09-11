using System;
using System.Collections.Generic;
using System.Data.Entity.Design.PluralizationServices;
using System.Globalization;
using System.IO;
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

        public T GetInstance<T>(object key) where T : class
        {
            return GetInstance<T>(key, null, "Id");
        }

        public T GetInstance<T>(object key, IEnumerable<string> columns) where T : class
        {
            return GetInstance<T>(key, columns, "Id");
        }

        public T GetInstance<T>(object key, IEnumerable<string> columns, string keyField) where T : class
        {
            var pluralizationService = PluralizationService.CreateService(new CultureInfo("en-US"));
            var collectionName = pluralizationService.Pluralize(typeof (T).Name);

            var type = typeof (T);
            var keyProperty = type.GetProperty(keyField);
            if (keyProperty == null)
            {
                keyProperty = type.GetProperty(type.Name + keyField);
                if (keyProperty == null)
                {
                    return null;
                }
            }
            try
            {
                var entries = (columns == null ? database.GetCollection<T>(collectionName).FindAll() : database.GetCollection<T>(collectionName).FindAll().SetFields(columns.ToArray())).ToList();
                foreach (var entry in entries)
                {
                    if (keyProperty.GetValue(entry).Equals(key))
                    {
                        Console.WriteLine("Returning " + type.Name + " with " + keyField + " " + key + " from MongoDbProvider");
                        return entry;
                    }
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Exception occured retrieving " + type.Name + " with " + keyField + " " + key + " from MongoDbProvider");
                //empty try/catch is a no-no, but thankfully this is just a test application ;)
            }
            return null;
        }

        public TResult GetValue<T, TResult>(object key, string propertyName) where T : class
        {
            var result = GetInstance<T>(key, new[] {propertyName});
            var propertyInfo = typeof (T).GetProperty(propertyName);
            return (TResult)propertyInfo.GetValue(result);
        }

        public TResult GetKey<T, TProperty, TResult>(string propertyName, TProperty propertyValue) where T : class
        {
            var result = GetInstance<T>(propertyValue, new[] { "Id", propertyName }, propertyName);
            var propertyInfo = typeof(T).GetProperty("Id");
            return (TResult)propertyInfo.GetValue(result);
        }
    }
}