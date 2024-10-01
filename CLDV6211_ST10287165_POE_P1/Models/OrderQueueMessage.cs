namespace CLDV6211_ST10287165_POE_P1.Models
{
    public class OrderQueueMessage
    {
        public string OrderId { get; set; }
        public string CustomerId { get; set; }
        public string Status { get; set; }  // "Pending", "Processed"
    }

}
