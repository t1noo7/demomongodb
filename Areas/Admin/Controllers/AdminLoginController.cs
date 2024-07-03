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

        private readonly IMongoCollection<Permissions> _permissions;

        public AdminLoginController(IMongoClient client)
        {
            var database = client.GetDatabase("DemoMongoDb");
            _adminAccounts = database.GetCollection<AdminAccounts>("AdminAccounts");
            _permissions = database.GetCollection<Permissions>("Permissions");
        }

        [Route("admin-login.html", Name = "AdminDangNhap")]
        [AllowAnonymous]
        public IActionResult Login(string returnUrl = null)
        {
            var accountID = HttpContext.Session.GetString("AccountId");
            if (accountID != null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("admin-login.html", Name = "AdminLoginPost")]
        public async Task<IActionResult> Login(LoginViewModel model)
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
                            new Claim("_id", admin._id)
                            // Add more claims as needed
                        };

                        // var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                        // var authProperties = new AuthenticationProperties
                        // {
                        //     RedirectUri = returnUrl
                        // };

                        // await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);
                        claims.Add(new Claim(ClaimTypes.Role, "Admin"));

                        var permissions = await _permissions.Find(p => p.RoleId == admin.RoleId).ToListAsync();

                        if (permissions != null)
                        {
                            foreach (var permission in permissions)
                            {
                                foreach (var task in permission.FunctionPermissions)
                                {
                                    claims.Add(new Claim($"FunctionId{task.FunctionId}", task.FunctionId));
                                    claims.Add(new Claim($"AccessPermission{task.FunctionId}", task.AccessPermission.ToString()));
                                    claims.Add(new Claim($"CanCreate{task.FunctionId}", task.CanCreate.ToString()));
                                    claims.Add(new Claim($"CanEdit{task.FunctionId}", task.CanEdit.ToString()));
                                    claims.Add(new Claim($"CanRead{task.FunctionId}", task.CanRead.ToString()));
                                    claims.Add(new Claim($"CanDelete{task.FunctionId}", task.CanDelete.ToString()));
                                }
                            }
                        }

                        ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, "AdminAuth");
                        ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                        await HttpContext.SignInAsync("AdminAuth", claimsPrincipal);

                        if (!string.IsNullOrEmpty(model.ReturnUrl))
                        {
                            return RedirectToAction("Index", "Home");
                        }
                    }
                }

                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }

            return View(model);
        }

        [HttpGet]
        //[Route("log-out-admin.html", Name = "DangXuat")]
        public IActionResult Logout()
        {
            HttpContext.SignOutAsync();
            HttpContext.Session.Remove("_id");
            return RedirectToAction("Login", "AdminLogin");
        }

        [Route("access-denied.html", Name = "AccessDenied")]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}