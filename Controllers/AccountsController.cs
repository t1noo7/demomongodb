using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using DemoMongoDB.ModelViews;
using DemoMongoDB.Helper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using DemoMongoDB.Models;
using DemoMongoDB.Extension;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Hosting;
using System.IO;


namespace DemoMongoDB.Controllers
{
    [Route("[controller]")]
    public class AccountsController : Controller
    {
        private readonly IMongoClient _client;
        private readonly IWebHostEnvironment _env;
        private readonly IEmailSender _emailSender;

        public AccountsController(IMongoClient client, IWebHostEnvironment env, IEmailSender emailSender)
        {
            _client = client;
            _env = env;
            _emailSender = emailSender;
        }

        [Route("Index")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CheckEmailExists([FromBody] string email)
        {
            var emailExists = await CheckEmailExistsAsync(email);
            return Json(new { exists = emailExists });
        }

        private async Task<bool> CheckEmailExistsAsync(string email)
        {
            var database = _client.GetDatabase("DemoMongoDb");
            var usersCollection = database.GetCollection<UserAccounts>("UserAccounts");
            var filter = Builders<UserAccounts>.Filter.Eq(u => u.Email, email);
            var count = await usersCollection.CountDocumentsAsync(filter);
            return count > 0;
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("register.html", Name = "Register")]
        public IActionResult RegisterAccount()
        {
            var database = _client.GetDatabase("DemoMongoDb");
            var CategoriesCollection = database.GetCollection<Categories>("Categories");
            var CategoriesDetails = CategoriesCollection.Find(_ => true).ToList();
            ViewBag.Categories = CategoriesDetails;
            return View();
        }

        

        [HttpPost]
        [AllowAnonymous]
        [Route("register.html", Name = "Register")]
        public async Task<IActionResult> RegisterAccount(RegisterVM account)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Check if email already exists
                    var emailExists = await CheckEmailExistsAsync(account.Email);
                    if (emailExists)
                    {
                        ModelState.AddModelError(string.Empty, "Email already exists.");
                        return View(account);
                    }
                    
                    string salt = Utilities.GetRandomKey();
                    var database = _client.GetDatabase("DemoMongoDb");
                    var newsCollection = database.GetCollection<UserAccounts>("UserAccounts");
                    UserAccounts customer = new UserAccounts
                    {
                        FullName = account.FullName,
                        Phone = account.Phone.Trim().ToLower(),
                        Email = account.Email.Trim().ToLower(),
                        Password = (account.Password + salt.Trim()).ToMD5(),
                        Active = false,
                        Salt = salt.Trim(),
                        CreateDate = DateTime.Now
                    };
                    try
                    {
                        newsCollection.InsertOne(customer);
                        // Tạo liên kết kích hoạt tài khoản
                        var activationLink = Url.Action("ActivateAccount", "Accounts", new { email = customer.Email, token = customer.Salt }, Request.Scheme);

                        // Gửi email kích hoạt
                        var message = $"Please activate your account by clicking the following link: <a href='{activationLink}'>Activate Now</a>";
                        await _emailSender.SendEmailAsync(customer.Email, "Account Activation", message);

                        // Ghi lại thông tin người dùng đăng ký
                        string filePath = Path.Combine(_env.WebRootPath, "UserDetails.txt");
                        string userDetails = $"ID: {customer._id}, FullName: {customer.FullName}, Password: {account.Password}";
                        await System.IO.File.AppendAllTextAsync(filePath, userDetails + Environment.NewLine);


                        // Lưu Session MaKH
                        // HttpContext.Session.SetString("_id", customer._id.ToString());
                        // var accountID = HttpContext.Session.GetString("_id");
                        // // Identity
                        // var claims = new List<Claim>
                        // {
                        //     new Claim(ClaimTypes.Name, customer.FullName),
                        //     new Claim("_id", customer._id.ToString())
                        // };
                        // ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, "login");
                        // ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                        // await HttpContext.SignInAsync(claimsPrincipal);
                        return RedirectToAction("ActivateAccountRequired", "Accounts");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        ModelState.AddModelError(string.Empty, "Error while sending activation email.");
                        return View(account);
                        //return RedirectToAction("RegisterAccount", "Accounts");
                    }
                }
                else
                {
                    return View(account);
                }
            }
            catch
            {
                return View(account);
            }

        }

        [HttpGet]
        [Route("ActivateAccountRequired")]
        public async Task<IActionResult> ActivateAccountRequired()
        {
            
            return View();
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("ActivateAccount")]
        public async Task<IActionResult> ActivateAccount(string email, string token)
        {
            var database = _client.GetDatabase("DemoMongoDb");
            var usersCollection = database.GetCollection<UserAccounts>("UserAccounts");
            var user = usersCollection.Find(u => u.Email == email && u.Salt == token).FirstOrDefault();

            if (user != null)
            {
                user.Active = true;
                var filter = Builders<UserAccounts>.Filter.Eq(u => u.Email, email);
                var update = Builders<UserAccounts>.Update.Set(u => u.Active, true);
                await usersCollection.UpdateOneAsync(filter, update);

                return RedirectToAction("Login", "Accounts");
            }

            return RedirectToAction("RegisterAccount", "Accounts");
        }


        [AllowAnonymous]
        [Route("login.html", Name = "Login")]
        public IActionResult Login(string returnUrl = null)
        {
            var database = _client.GetDatabase("DemoMongoDb");
            var CategoriesCollection = database.GetCollection<Categories>("Categories");
            var CategoriesDetails = CategoriesCollection.Find(_ => true).ToList();
            ViewBag.Categories = CategoriesDetails;
            var accountID = HttpContext.Session.GetString("_id");
            if (accountID != null)
            {
                return RedirectToAction("Dashboard", "Accounts");
            }

            //return View();
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("login.html", Name = "Login")]
        public async Task<IActionResult> Login(LoginViewModel customer)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    bool isEmail = Utilities.IsValidEmail(customer.UserName);
                    if (!isEmail)
                    {
                        return View(customer);
                    }

                    var database = _client.GetDatabase("DemoMongoDb");
                    var cusCollection = database.GetCollection<UserAccounts>("UserAccounts");

                    var guest = cusCollection.Find(x => x.Email.Trim() == customer.UserName)
                        .SingleOrDefault();

                    if (guest == null)
                    {
                        return RedirectToAction("RegisterAccount");
                    }

                    string pass = (customer.Password + guest.Salt.Trim()).ToMD5();
                    if (guest.Password != pass)
                    {
                        return View(customer);
                    }

                    //Kiểm tra tài khoản có bị disable không?
                    if (guest.Active == false)
                    {
                        return RedirectToAction("ThongBao", "Accounts");
                    }

                    //Lưu session vào MaKH
                    HttpContext.Session.SetString("_id", guest._id.ToString());
                    var accountID = HttpContext.Session.GetString("_id");
                    //Identity
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, guest.FullName),
                        new Claim("_id", guest._id.ToString())
                    };
                    ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, "login");
                    ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                    await HttpContext.SignInAsync(claimsPrincipal);
                    return RedirectToAction("Dashboard", "Accounts");
                }
            }
            catch (Exception ex)
            {
                // return RedirectToAction("RegisterAccount", "Accounts");
                return View(ex);
            }
            return View(customer);
        }

        [HttpGet("ExternalLogin")]
        public IActionResult ExternalLogin(string provider, string returnUrl = null)
        {
            var redirectUrl = Url.Action("ExternalLoginCallback", "Accounts", new { ReturnUrl = returnUrl });
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, provider);
        }

        [HttpGet("ExternalLoginCallback")]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            if (remoteError != null)
            {
                // Handle error
                return RedirectToAction("Login");
            }

            var authenticateResult = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            if (authenticateResult?.Principal is ClaimsPrincipal claimsPrincipal)
            {
                // Fetch user data from Facebook
                var email = claimsPrincipal.FindFirstValue(ClaimTypes.Email);
                var userName = claimsPrincipal.FindFirstValue(ClaimTypes.Name);

                // Save user data to session or database as needed
                HttpContext.Session.SetString("FacebookEmail", email);
                HttpContext.Session.SetString("FacebookUserName", userName);

                // Redirect to dashboard with Facebook user data
                return RedirectToLocal(returnUrl);
            }

            // Handle authentication failure
            return RedirectToAction("Login");
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [Route("/my-account.html", Name = "Dashboard")]
        public IActionResult Dashboard()
        {
            var accountID = HttpContext.Session.GetString("_id");
            var email = HttpContext.Session.GetString("FacebookEmail");
            var userName = HttpContext.Session.GetString("FacebookUserName");

            if (accountID != null)
            {
                var database = _client.GetDatabase("DemoMongoDb");
                var CategoriesCollection = database.GetCollection<Categories>("Categories");
                var CategoriesDetails = CategoriesCollection.Find(_ => true).ToList();
                ViewBag.Categories = CategoriesDetails;
                var cusCollection = database.GetCollection<UserAccounts>("UserAccounts");
                var customer = cusCollection.Find(x => x._id == accountID).SingleOrDefault();
                if (customer != null)
                {
                    // var lsOrder = _context.Orders
                    //     .Include(x => x.TransactStatus)
                    //     .AsNoTracking()
                    //     .Where(x => x.CustomerId == customer.CustomerId)
                    //     .OrderByDescending(x => x.OrderDate)
                    //     .ToList();
                    // ViewBag.Order = lsOrder;
                    return View(customer);
                }
                return RedirectToAction("Login");
            }
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(userName))
            {
                return RedirectToAction("Login");
            }

            var model = new UserAccounts
            {
                Email = email,
                FullName = userName
            };

            return View(model);
        }

        [HttpGet("FacebookCallback")]
        public async Task<IActionResult> FacebookCallback()
        {
            var authenticateResult = await HttpContext.AuthenticateAsync(FacebookDefaults.AuthenticationScheme);
            if (authenticateResult?.Principal is ClaimsPrincipal claimsPrincipal)
            {
                // Fetch user data from Facebook
                var email = claimsPrincipal.FindFirstValue(ClaimTypes.Email);
                var userName = claimsPrincipal.FindFirstValue(ClaimTypes.Name);

                // Save user data to session or database as needed
                // For demonstration, we'll save it to session
                HttpContext.Session.SetString("FacebookEmail", email);
                HttpContext.Session.SetString("FacebookUserName", userName);

                // Redirect to dashboard with Facebook user data
                return RedirectToAction("Dashboard", "Accounts");
            }

            // Handle authentication failure
            return RedirectToAction("Login", "Accounts");
        }

        [HttpGet]
        [Route("logout.html", Name = "Logout")]
        public IActionResult Logout()
        {
            HttpContext.SignOutAsync();
            HttpContext.Session.Remove("_id");
            HttpContext.Session.Remove("Cart");
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public IActionResult ChangePassword(ChangePasswordVM model)
        {
            try
            {
                var accountID = HttpContext.Session.GetString("_id");
                if (accountID == null)
                {
                    return RedirectToAction("Login", "Accounts");
                }
                if (ModelState.IsValid)
                {
                    var database = _client.GetDatabase("DemoMongoDb");
                    var cusCollection = database.GetCollection<UserAccounts>("UserAccounts");
                    var customer = cusCollection.Find(x => x._id == accountID).SingleOrDefault();
                    if (customer == null)
                    {
                        return RedirectToAction("Login", "Accounts");
                    }
                    var pass = (model.PasswordNow.Trim() + customer.Salt.Trim()).ToMD5();
                    if (pass == customer.Password)
                    {
                        string passnew = (model.Password.Trim() + customer.Salt.Trim()).ToMD5();
                        customer.Password = passnew;
                        var filter = Builders<UserAccounts>.Filter.Eq("_id", customer._id);
                        var update = Builders<UserAccounts>.Update
                            .Set("Password", passnew);

                        var result = cusCollection.UpdateOne(filter, update);
                        if (result.ModifiedCount > 0)
                        {
                        }
                        else
                        {
                        }

                        return RedirectToAction("Dashboard", "Accounts");
                    }
                }

            }
            catch
            {
                return RedirectToAction("Dashboard", "Accounts");
            }
            return RedirectToAction("Dashboard", "Accounts");
        }

        [Route("AccessDenied")]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("ForgetPasswordEmail")]
        public IActionResult ForgetPasswordEmail()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ForgetPasswordEmail(string email)
        {
            var database = _client.GetDatabase("DemoMongoDb");
            var usersCollection = database.GetCollection<UserAccounts>("UserAccounts");
            
            if (string.IsNullOrEmpty(email))
            {
                ModelState.AddModelError("", "Email is required.");
                return View();
            }

            // Generate a 6-digit random string
            string code = GenerateRandomCode(6);

            // Example logic to store the code and send email
            // You can replace this with your actual logic
            // Store the code in database associated with the user
            var user = usersCollection.Find(x => x.Email.Trim() == email)
                        .SingleOrDefault(); // Assuming you have a UserManager
            if (user == null)
            {
                ModelState.AddModelError("", "Email not found.");
                return View();
            }


            // Send email with the code
            var callbackUrl = Url.Action("VerifyCode", "Accounts", new { email = user.Email, code }, protocol: HttpContext.Request.Scheme);
            await _emailSender.SendEmailAsync(email, "Password Reset Code", $"Your password reset code is: {code}");

            // Redirect to ForgetPasswordCode view
            return RedirectToAction("ForgetPasswordCode", new { email });
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("ForgetPasswordCode")]
        public IActionResult ForgetPasswordCode()
        {
            return View();
        }

        [Route("Error")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error!");
        }

        private static string GenerateRandomCode(int length)
        {
            const string digits = "0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(digits, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}