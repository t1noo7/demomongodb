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


namespace DemoMongoDB.Controllers
{
    [Route("[controller]")]
    public class AccountsController : Controller
    {
        private readonly IMongoClient _client;

        public AccountsController(IMongoClient client)
        {
            _client = client;
        }

        [Route("Index")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("register.html", Name = "Register")]
        public IActionResult RegisterAccount()
        {
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
                    string salt = Utilities.GetRandomKey();
                    var database = _client.GetDatabase("DemoMongoDb");
                    var newsCollection = database.GetCollection<UserAccounts>("UserAccounts");
                    UserAccounts customer = new UserAccounts
                    {
                        FullName = account.FullName,
                        Phone = account.Phone.Trim().ToLower(),
                        Email = account.Email.Trim().ToLower(),
                        Password = (account.Password + salt.Trim()).ToMD5(),
                        Active = true,
                        Salt = salt.Trim(),
                        CreateDate = DateTime.Now

                    };
                    try
                    {
                        newsCollection.InsertOne(customer);
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
                        return RedirectToAction("Login", "Accounts");
                    }
                    catch (Exception ex)
                    {
                        return RedirectToAction("RegisterAccount", "Accounts");
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


        [AllowAnonymous]
        [Route("login.html", Name = "Login")]
        public IActionResult Login(string returnUrl = null)
        {
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
                return RedirectToAction("RegisterAccount", "Accounts");
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

        [Route("Error")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error!");
        }
    }
}