using DemoMongoDB.Models;

namespace DemoMongoDB.ModelViews
{
    public class CartItem
    {
        public Courses courses { get; set; }
        public int Quantity { get; set; }

    }
}
