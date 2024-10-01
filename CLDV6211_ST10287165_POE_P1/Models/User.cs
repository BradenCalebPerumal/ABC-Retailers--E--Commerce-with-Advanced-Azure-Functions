using System.ComponentModel.DataAnnotations;

namespace CLDV6211_ST10287165_POE_P1.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
    }
}
