using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using DemoMongoDB.Models;
using DemoMongoDB.ModelViews;
using DemoMongoDB.Extension;
using MongoDB.Driver;

public class CartController : Controller
{
    private readonly IMongoClient _client;

    public CartController(IMongoClient client)
    {
        _client = client;
    }
    public List<CartItem> CartItems
    {
        get
        {
            var cart = HttpContext.Session.Get<List<CartItem>>("CartItems");
            if (cart == default(List<CartItem>))
            {
                cart = new List<CartItem>();
            }
            return cart;
        }
    }

    [Route("/cart.html", Name = "Cart")]
    public IActionResult Index()
    {
        //get the number of items in the cart and pass it to ViewBag
        ViewBag.CartCount = CartItems.Count;
        return View(CartItems); // Trả về view với danh sách sản phẩm trong giỏ hàng
    }

    [HttpPost]
    [Route("api/cart/add")]
    public IActionResult AddToCart(string productID, int? quantity)
    {
        var database = _client.GetDatabase("DemoMongoDb");
        try
        {
            List<CartItem> cartItems = CartItems;
            //Thêm sản phẩm vào giỏ hàng
            CartItem item = cartItems.SingleOrDefault(p => p.courses._id == productID);
            if (item != null)//đã có --) cập nhật số lượng
            {
                if (quantity.HasValue)
                {
                    item.Quantity = quantity.Value;
                }
                else
                {
                    item.Quantity++;
                }
            }
            else
            {
                var coursesCollection = database.GetCollection<Courses>("Courses");
                Courses courses = coursesCollection.Find(p => p._id == productID).SingleOrDefault();
                item = new CartItem
                {
                    Quantity = quantity.HasValue ? quantity.Value : 1,
                    courses = courses
                };
                cartItems.Add(item);//thêm vào giỏ
            }
            //lưu lại session
            HttpContext.Session.Set<List<CartItem>>("CartItems", cartItems);
            // return Json(new { success = true });
            return RedirectToAction("Index", "Home");
        }
        catch (Exception ex)
        {
            // return Json(new { success = false });
            Console.WriteLine($"Error adding to cart: {ex.Message}");
            return RedirectToAction("Index", "Home");
        }
    }

    [HttpPost]
    [Route("api/cart/update")]
    public IActionResult UpdateCart(string productID, int? quantity)
    {
        // lấy giỏ hàng ra để xử lý
        var cart = HttpContext.Session.Get<List<CartItem>>("CartItems");
        try
        {
            if (cart != null)
            {
                CartItem item = cart.SingleOrDefault(p => p.courses._id == productID);
                if (item != null && quantity.HasValue)//đã có --) cập nhật số lượng
                {
                    item.Quantity = quantity.Value;
                }
                //lưu lại session
                HttpContext.Session.Set<List<CartItem>>("CartItems", cart);
            }
            return Json(new { success = true });
        }
        catch
        {
            return Json(new { success = false });
        }
    }

    [HttpPost]
    [Route("api/cart/remove")]
    public ActionResult Remove(string productID)
    {
        try
        {
            List<CartItem> cartItems = CartItems;
            CartItem item = cartItems.SingleOrDefault(p => p.courses._id == productID);
            if (item != null)
            {
                cartItems.Remove(item);
            }
            // Lưu lại session
            HttpContext.Session.Set<List<CartItem>>("CartItems", cartItems);
            return Json(new { success = true });
        }
        catch
        {
            return Json(new { success = false });
        }
    }
}
