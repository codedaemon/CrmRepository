using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrmRepository.Entities;

namespace CrmRepository.Agents
{
    public class StoreAgent : IAgent
    {
        public object GetInstance(object key)
        {
            return GetInstance(key, typeof (Store).GetProperties().Select(p => p.Name));
            
        }

        public object GetInstance(object key, IEnumerable<string> columns)
        {
            if (!(key is Guid))
            {
                return null;
            }
            Console.WriteLine("Returning Store " + key + " from StoreAgent");
            return new Store
            {
                StoreId = columns.Contains("StoreId") ? (Guid) key : new Guid(),
                Name = columns.Contains("Name") ? "Testsjappa" : null,
                Address = columns.Contains("Address") ? "Olaveien 14, 1234 GOKK" : null,
                OpeningHours = columns.Contains("OpeningHours") ? "07-23" : null
            };
        }

        public object GetValue(object key, string propertyName)
        {
            if (!(key is Guid))
            {
                return null;
            }
            switch (propertyName)
            {
                case "StoreId": return (Guid)key;
                case "Name": return "Testsjappa";
                case "Address": return "Olaveien 14, 1234 GOKK";
                case "OpeningHours": return "07-23";
                default: return null;
            }
        }

        public object GetKey(string propertyName, object propertyValue)
        {
            if ((propertyName == "Name" && propertyValue.ToString() == "Testsjappa") ||
                (propertyName == "Address" && propertyValue.ToString() == "Olaveien 14, 1234 GOKK") ||
                (propertyName == "OpeningHours" && propertyValue.ToString() == "07-23"))
                return Guid.Parse("4a5bdc79-207c-454f-b473-26a9382cd53c");
            return new Guid();
        }
    }
}
