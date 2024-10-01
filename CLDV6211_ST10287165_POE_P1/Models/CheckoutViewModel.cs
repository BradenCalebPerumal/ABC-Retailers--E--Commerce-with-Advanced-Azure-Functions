using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CLDV6211_ST10287165_POE_P1.Models
{
    public class CheckoutViewModel
    {
        public List<CartItem> CartItems { get; set; }
        public Order Order { get; set; }
    }

}
