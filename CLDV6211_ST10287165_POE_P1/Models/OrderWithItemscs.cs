namespace CLDV6211_ST10287165_POE_P1.Models
{
    public class OrderWithItems
    {
        public Order Order { get; set; }
        public IEnumerable<OrderItem> OrderItems { get; set; }
    }
}
