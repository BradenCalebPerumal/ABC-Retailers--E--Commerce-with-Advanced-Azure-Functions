namespace CLDV6211_ST10287165_POE_P1.Models
{
    public class OrderConfirmationViewModel
    {
        public string OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public string OrderStatus { get; set; }
        public string ShippingAddress { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
        public string PaymentMethod { get; set; }
        public List<OrderItem> OrderItems { get; set; }
    }

}

