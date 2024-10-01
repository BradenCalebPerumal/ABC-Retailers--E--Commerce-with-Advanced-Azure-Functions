using Azure;
using Azure.Data.Tables;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CLDV6211_ST10287165_POE_P1.Models
{
    public class Product : ITableEntity
    {
        [BindNever]
        public string PartitionKey { get; set; }  // Set automatically, not bound to the form

        [BindNever]
        public string RowKey { get; set; }  // Set automatically, not bound to the form

        [Required]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }

        [BindNever]
        public string ClientId { get; set; }  // Set automatically, not bound to the form

        [Required]
        [DataType(DataType.Currency)]
        public Double Price { get; set; }

        public string Category { get; set; }

        [Required(ErrorMessage = "Please select an image type.")]
        public string ImageType { get; set; }

        public string? ImageUrl { get; set; }  // Nullable URL pointing to the image in Blob Storage

        [Required]
        public int ?Quantity { get; set; }

        [NotMapped] // This attribute prevents the property from being stored in the database
        public string InStock
        {
            get { return Quantity > 0 ? "In Stock" : "Out of Stock"; }
        }

        [NotMapped]
        public IFormFile? ImageUpload { get; set; }  // Nullable for handling image uploads in the UI

        public DateTimeOffset? Timestamp { get; set; }  // For Azure Table Storage tracking
        public ETag ETag { get; set; }  // For concurrency handling in Azure Table Storage
    }
}
