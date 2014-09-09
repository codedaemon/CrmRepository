namespace CrmRepository.Entities
{
    public class OrderLine
    {
        public int Qty { get; set; }
        public decimal Cost { get; set; }
        public string ItemName { get; set; }
    }
}