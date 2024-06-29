using System.Collections.Generic;
using DemoMongoDB.ModelViews;

namespace DemoMongoDB.Models
{
    public class CheckoutViewModel
    {
        public List<CartItem> CartItems { get; set; }
        public double OrderTotal { get; set; }
        public string OrderType { get; set; }
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public string OrderDescription { get; set; }
        public string OrderId { get; set; }
        public string OrderInfo { get; set; }
    }
}