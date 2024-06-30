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
        private IMomoService _momoService;
        private readonly IEmailSender _emailSender;

        public OrdersController(IMongoClient client, IVnPayService vnPayService, IMomoService momoService, IEmailSender emailSender)
        {
            var database = client.GetDatabase("DemoMongoDb");
            _ordersCollection = database.GetCollection<Orders>("Orders");
            _coursesCollection = database.GetCollection<Courses>("Courses");
            _vnPayService = vnPayService;
            _momoService = momoService;
            _emailSender = emailSender;
        }

        // GET: Orders/Checkout
        [Route("checkout.html", Name="CheckOut")]
        public IActionResult Checkout()
        {
            // if (!User.Identity.IsAuthenticated)
            // {
            //     return RedirectToAction("Login", "Account"); // Redirect to your login action
            // }
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
        [Route("checkout.html", Name = "CheckOut")]
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
                    Thumb = item.courses.Thumb,
                    Price = item.courses.Price,
                    Quantity = item.Quantity
                }).ToList();

                // Calculate total amount
                // order.TotalAmount = cartItems.Sum(item => item.Quantity * item.courses.Price);
                order.Status = "Chờ xác nhận thông tin thanh toán";

                // Save order to MongoDB
                await _ordersCollection.InsertOneAsync(order);

                // Clear cart items from session after successful checkout
                HttpContext.Session.Remove("CartItems");

                var emailSubject = "Xác nhận Đăng ký Khóa học Thành công";
                var emailBody = GenerateEmailBody(order);
                await _emailSender.SendEmailAsync(order.CustomerEmail, emailSubject, emailBody);

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

        public IActionResult vnPayCreatePaymentUrl(CheckoutViewModel model)
        {
            var url = _vnPayService.CreatePaymentUrl(model, HttpContext);

            return Redirect(url);
        }

        public async Task<IActionResult> vnPayPaymentCallback()
        {
            var response = _vnPayService.PaymentExecute(Request.Query);
            await _ordersCollection.InsertOneAsync(new Orders
            {
                CustomerName = response.CustomerName,
                CustomerEmail = response.CustomerEmail,
                PaymentContent = response.OrderDescription,
                TotalAmount = Convert.ToDouble(response.TotalAmount),
                Status = "Đã thanh toán với VNPay"
            });
            HttpContext.Session.Remove("CartItems");

            var emailSubject = "Xác nhận Đăng ký Khóa học Thành công";
            var emailBody = GenerateEmailBody(response);
            await _emailSender.SendEmailAsync(response.CustomerEmail, emailSubject, emailBody);

            // Redirect to order confirmation or thank you page
            return RedirectToAction("ThankYou", new { orderId = response.OrderId, customerEmail = response.CustomerEmail });
            // return Json(response);
        }

        [HttpPost]
        public async Task<IActionResult> MoMoCreatePaymentUrl(CheckoutViewModel model)
        {
            var response = await _momoService.CreatePaymentAsync(model);
            return Redirect(response.PayUrl);
        }

        [HttpGet]
        public IActionResult MoMoPaymentCallBack()
        {
            var response = _momoService.PaymentExecuteAsync(HttpContext.Request.Query);
            _ordersCollection.InsertOne(new Orders
            {
                CustomerName = response.CustomerName,
                CustomerEmail = response.CustomerEmail,
                PaymentContent = response.OrderInfo,
                TotalAmount = Convert.ToDouble(response.Amount),
                Status = "Đã thanh toán với MoMo"
            });
            return View(response);
        }

        private string GenerateEmailBody(Orders order)
        {
            var courseDetails = string.Join("<br>", order.OrderDetails.Select(od =>
                $"- Tên Khóa học: {od.CourseTitle}<br>- Mã Khóa học: {od.CourseId}<br>- Giá: {od.Price} VND<br>- Số lượng: {od.Quantity}<br>- Tổng cộng: {od.Quantity * od.Price} VND<br>"
            ));

            return $@"
                Chào {order.CustomerName},<br><br>

                Cảm ơn bạn đã đăng ký khóa học tại E-Courses! Chúng tôi rất vui mừng được chào đón bạn tham gia khóa học. Dưới đây là thông tin chi tiết về khóa học mà bạn đã đăng ký:<br><br>

                <b>Thông tin Khóa học:</b><br>
                {courseDetails}<br>

                <b>Thông tin Người đăng ký:</b><br>
                - Họ tên: {order.CustomerName}<br>
                - Email: {order.CustomerEmail}<br><br>

                <b>Thông tin Thanh toán:</b><br>
                - Ngày đăng ký: {order.OrderDate.ToString("dd/MM/yyyy")}<br>
                - Phương thức thanh toán: {order.Status}<br><br>

                <b>Hướng dẫn Tham gia:</b><br>
                - Để bắt đầu khóa học, bạn có thể truy cập vào trang web của chúng tôi tại: [Liên kết đến trang khóa học].<br>
                - Nếu bạn có bất kỳ câu hỏi nào, vui lòng liên hệ với chúng tôi qua email [Email hỗ trợ] hoặc số điện thoại [Số điện thoại hỗ trợ].<br><br>

                Một lần nữa, cảm ơn bạn đã chọn [Tên Công ty/Trang web]! Chúng tôi hy vọng rằng bạn sẽ có trải nghiệm học tập thú vị và bổ ích.<br><br>

                Trân trọng,<br>
                [Đội ngũ hỗ trợ]<br>
                E-courses International Educational Organization<br>
                Email: support-center@e-courses.com<br>
                Phone: 0123456789<br>
            ";
        }
        private string GenerateEmailBody(PaymentResponseModel response)
        {
            // var courseDetails = string.Join("<br>", order.OrderDetails.Select(od =>
            //     $"- Tên Khóa học: {od.CourseTitle}<br>- Mã Khóa học: {od.CourseId}<br>- Giá: {od.Price} VND<br>- Số lượng: {od.Quantity}<br>- Tổng cộng: {od.Quantity * od.Price} VND<br>"
            // ));

            return $@"
                Chào {response.CustomerName},<br><br>

                Cảm ơn bạn đã đăng ký khóa học tại E-Courses! Chúng tôi rất vui mừng được chào đón bạn tham gia khóa học. Dưới đây là thông tin chi tiết về khóa học mà bạn đã đăng ký:<br><br>

                <b>Thông tin Khóa học:</b><br>
                E-Courses<br>

                <b>Thông tin Người đăng ký:</b><br>
                - Họ tên: {response.CustomerName}<br>
                - Email: {response.CustomerEmail}<br><br>

                <b>Thông tin Thanh toán:</b><br>
                - Ngày đăng ký: {response.OrderDate.ToString("dd/MM/yyyy")}<br>
                - Phương thức thanh toán: Đã thanh toán với VNPay<br><br>

                <b>Hướng dẫn Tham gia:</b><br>
                - Để bắt đầu khóa học, bạn có thể truy cập vào trang web của chúng tôi tại: [Liên kết đến trang khóa học].<br>
                - Nếu bạn có bất kỳ câu hỏi nào, vui lòng liên hệ với chúng tôi qua email [Email hỗ trợ] hoặc số điện thoại [Số điện thoại hỗ trợ].<br><br>

                Một lần nữa, cảm ơn bạn đã chọn [Tên Công ty/Trang web]! Chúng tôi hy vọng rằng bạn sẽ có trải nghiệm học tập thú vị và bổ ích.<br><br>

                Trân trọng,<br>
                [Đội ngũ hỗ trợ]<br>
                E-courses International Educational Organization<br>
                Email: support-center@e-courses.com<br>
                Phone: 0123456789<br>
            ";
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
