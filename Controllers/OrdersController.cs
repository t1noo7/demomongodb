using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using DemoMongoDB.Models;
using MongoDB.Driver;
using DemoMongoDB.ModelViews;
using DemoMongoDB.Extension;
using DemoMongoDB.Services;

namespace DemoMongoDB.Controllers
{
    public class OrdersController : Controller
    {
        private readonly IMongoCollection<Orders> _ordersCollection;
        private readonly IMongoCollection<Courses> _coursesCollection;
        private readonly IVnPayService _vnPayService;

        public OrdersController(IMongoClient client, IVnPayService vnPayService)
        {
            var database = client.GetDatabase("DemoMongoDb");
            _ordersCollection = database.GetCollection<Orders>("Orders");
            _coursesCollection = database.GetCollection<Courses>("Courses");
            _vnPayService = vnPayService;
        }

        // GET: Orders/Checkout
        public IActionResult Checkout()
        {
            // Retrieve cart items from session
            List<CartItem> cartItems = HttpContext.Session.Get<List<CartItem>>("CartItems");
            if (cartItems == null || cartItems.Count == 0)
            {
                // Redirect to cart or handle empty cart scenario
                return RedirectToAction("Cart", "Cart");
            }

            // Map cart items to OrderDetails
            var orderDetails = cartItems.Select(item => new OrderDetails
            {
                CourseId = item.courses._id,
                CourseTitle = item.courses.Title,
                Price = item.courses.Price,
                Quantity = item.Quantity
            }).ToList();

            // Calculate total amount
            double totalAmount = cartItems.Sum(item => item.Quantity * item.courses.Price);

            // Prepare ViewModel for checkout page
            var model = new CheckoutViewModel
            {
                CartItems = cartItems,
                OrderTotal = totalAmount
            };

            return View(model);
        }

        // POST: Orders/Checkout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout([Bind("CustomerName,CustomerEmail")] Orders order)
        {
            try
            {
                // Retrieve cart items from session
                List<CartItem> cartItems = HttpContext.Session.Get<List<CartItem>>("CartItems");
                if (cartItems == null || cartItems.Count == 0)
                {
                    return RedirectToAction("Cart", "Cart");
                }

                // Map cart items to OrderDetails
                order.OrderDetails = cartItems.Select(item => new OrderDetails
                {
                    CourseId = item.courses._id,
                    CourseTitle = item.courses.Title,
                    Price = item.courses.Price,
                    Quantity = item.Quantity
                }).ToList();

                // Calculate total amount
                order.TotalAmount = cartItems.Sum(item => item.Quantity * item.courses.Price);

                // Save order to MongoDB
                await _ordersCollection.InsertOneAsync(order);

                // Clear cart items from session after successful checkout
                HttpContext.Session.Remove("CartItems");

                // Redirect to order confirmation or thank you page
                return RedirectToAction("ThankYou", new { orderId = order._id, customerEmail = order.CustomerEmail });
            }
            catch (Exception ex)
            {
                // Handle error scenario
                Console.WriteLine($"Error during checkout: {ex.Message}");
                ModelState.AddModelError("", "An error occurred during checkout.");
            }


            // If model state is not valid, return to checkout page with validation errors
            return View(order);
        }

        public IActionResult CreatePaymentUrl(CheckoutViewModel model)
        {
            var url = _vnPayService.CreatePaymentUrl(model, HttpContext);

            return Redirect(url);
        }

        public IActionResult PaymentCallback()
        {
            var response = _vnPayService.PaymentExecute(Request.Query);

            return Json(response);
        }

        // GET: Orders/ThankYou
        public IActionResult ThankYou(string orderId, string customerEmail)
        {
            ViewBag.OrderId = orderId;
            ViewBag.CustomerEmail = customerEmail;
            return View();
        }
    }
}
