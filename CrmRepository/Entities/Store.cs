using System;

namespace CrmRepository.Entities
{
    public class Store
    {
        public Guid StoreId { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string OpeningHours { get; set; }
    }
}