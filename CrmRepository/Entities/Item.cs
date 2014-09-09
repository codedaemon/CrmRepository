using System;

namespace CrmRepository.Entities
{
    public class Item
    {
        public Guid WronglyNamedId { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int QtyOnStock { get; set; }
    }
}