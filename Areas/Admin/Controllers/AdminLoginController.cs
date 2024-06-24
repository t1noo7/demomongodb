using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using DemoMongoDB.Models;
using DemoMongoDB.ModelViews;
using DemoMongoDB.Extension;

namespace DemoMongoDB.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AdminLoginController : Controller
    {
        private readonly IMongoCollection<AdminAccounts> _adminAccounts;

        public AdminLoginController(IMongoClient client)
        {
            var database = client.GetDatabase("DemoMongoDb");
            _adminAccounts = database.GetCollection<AdminAccounts>("AdminAccounts");
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("admin-login.html", Name = "AdminLogin")]
        public IActionResult Login(string returnUrl = "/Admin")
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("admin-login.html", Name = "AdminLoginPost")]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = "/Admin")
        {
            if (ModelState.IsValid)
            {
                var admin = await _adminAccounts
                    .Find(a => a.Email == model.UserName && a.Active)
                    .FirstOrDefaultAsync();

                if (admin != null)
                {
                    var password = (model.Password + admin.Salt.Trim()).ToMD5();

                    if (admin.Password == password)
                    {
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name, admin.Email),
                            new Claim(ClaimTypes.Role, admin.Role)
                            // Add more claims as needed
                        };

                        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                        var authProperties = new AuthenticationProperties
                        {
                            RedirectUri = returnUrl
                        };

                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

                        return RedirectToAction("Index", "Home");
                    }
                }

                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View(model);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "AdminLogin");
        }

        [Route("access-denied.html", Name = "AccessDenied")]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
