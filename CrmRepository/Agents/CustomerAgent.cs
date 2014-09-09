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
            if (!(key is Guid))
            {
                return null;
            }
            Console.WriteLine("Returning Store " + key + " from StoreAgent");
            return new Store
            {
                StoreId = (Guid) key,
                Name = "Testsjappa",
                Address = "Olaveien 14, 1234 GOKK",
                OpeningHours = "07-23"
            };
        }
    }
}
