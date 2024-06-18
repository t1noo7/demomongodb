using System.Collections.Generic;
using DemoMongoDB.ModelViews;

namespace DemoMongoDB.Models
{
    public class CheckoutViewModel
    {
        public List<CartItem> CartItems { get; set; }
        public int OrderTotal { get; set; }
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
    }
}