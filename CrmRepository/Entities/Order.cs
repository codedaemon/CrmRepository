using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmRepository.Entities
{
    public class Order
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string DeliveryDate { get; set; }
        public List<OrderLine> OrderLines { get; set; }
    }
}
