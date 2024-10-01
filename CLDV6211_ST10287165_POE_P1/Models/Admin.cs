using Azure;
using Azure.Data.Tables;
using System.ComponentModel.DataAnnotations.Schema;

namespace CLDV6211_ST10287165_POE_P1.Models
{
    public class Admin : ITableEntity
    {
        [NotMapped]
        public int AdminID { get; set; } // Not used as Azure Table Storage doesn't require it
        public string AdminEmail { get; set; }
        public string AdminPasswordHash { get; set; }
      
        public string AdminPassword { get; set; } // Plain-text password for display/editing

        // Azure Table Storage properties
        public string PartitionKey { get; set; } = "Admin"; // Static partition key for all admins
        public string RowKey { get; set; } // Unique identifier, such as email or a GUID
        public DateTimeOffset? Timestamp { get; set; }
        [NotMapped]
        public ETag ETag { get; set; }
    }
}
