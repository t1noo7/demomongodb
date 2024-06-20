using Microsoft.AspNetCore.Mvc;
using DiChoSaiGon.Models;
using AspNetCoreHero.ToastNotification.Abstractions;
using DiChoSaiGon.Helpper;
using DiChoSaiGon.ModelViews;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;
using DiChoSaiGon.Areas.Admin.ViewModel;
using DiChoSaiGon.Extensions;
using static System.Net.WebRequestMethods;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace DiChoSaiGon.Controllers
{
    [Authorize(Roles = "Admin", AuthenticationSchemes = "AdminAuth")]
    [Area("Admin")]
    public class AdminLoginController : Controller
    {
        private readonly MarketContext _context;
        public INotyfService _notyfService { get; }
        public AdminLoginController(MarketContext context, INotyfService notifyService)
        {
            _context = context;
            _notyfService = notifyService;
        }

        public IActionResult AdminLocation()
        {
            var accountID = HttpContext.Session.GetString("AccountId");
            UserProfileViewModel userProfile = null;
            if (accountID != null)
            {
                var adminAccount = _context.Accounts.AsNoTracking()
                    .SingleOrDefault(x => x.AccountId == Convert.ToInt32(accountID));

                if (adminAccount != null)
                {
                    userProfile = new UserProfileViewModel
                    {
                        FullName = adminAccount.FullName,
                        RoleName = adminAccount.Role.RoleName
                    };
                }
            }

            // Set userProfile in ViewData to make sure it's available in the view
            ViewData["UserProfile"] = userProfile;

            if (userProfile != null)
            {
                // If userProfile is not null, return the partial view with the userProfile data
                return PartialView("_NavWrapPartialView", userProfile);
            }
            else
            {
                // If userProfile is null or accountID is null, redirect to login action
                return RedirectToAction("Login");
            }
        }


        [AllowAnonymous]
        [Route("/admin-login.html", Name = "Admin Login")]
        public IActionResult Login(string returnUrl = null)
        {
            var accountID = HttpContext.Session.GetString("AccountId");
            if (accountID != null)
            {
                return RedirectToAction("Index", "Home");
            }

            //return View();
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("/admin-login.html", Name = "Admin Login")]
        public async Task<IActionResult> Login(LoginViewModel accountAdmin)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    bool isEmail = Utilities.IsValidEmail(accountAdmin.UserName);
                    if (!isEmail)
                    {
                        return View(accountAdmin);
                    }

                    var managerCustomer = _context.Accounts.AsNoTracking()
                        .SingleOrDefault(x => x.Email.Trim() == accountAdmin.UserName);

                    if (managerCustomer == null)
                    {
                        _notyfService.Success("Your Information is unavailable!");
                        return RedirectToAction("RegisterAccount", "Accounts");
                    }

                    string pass = (accountAdmin.Password + managerCustomer.Salt.Trim()).ToMD5();
                    if (managerCustomer.Password != pass)
                    {
                        _notyfService.Success("Invalid Login Information!");
                        return View(accountAdmin);
                    }

                    //Kiểm tra tài khoản có bị disable không?
                    if (managerCustomer.Active == false)
                    {
                        return RedirectToAction("ThongBao", "Accounts");
                    }

                    //Lưu session vào MaKH
                    HttpContext.Session.SetString("AccountId", managerCustomer.AccountId.ToString());
                    var accountID = HttpContext.Session.GetString("AccountId");
                    //Identity
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, managerCustomer.FullName),
                        new Claim("AccountId", managerCustomer.AccountId.ToString())
                    };
                    if (managerCustomer.RoleId == 1)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, "Admin"));
                        ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, "AdminAuth");
                        ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                        await HttpContext.SignInAsync("AdminAuth", claimsPrincipal);
                    }
                    else if (managerCustomer.RoleId == 2)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, "Staff"));
                        ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, "StaffAuth");
                        ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                        await HttpContext.SignInAsync("StaffAuth", claimsPrincipal);
                    }
                    _notyfService.Success("Login Successful!");
                    if (!string.IsNullOrEmpty(accountAdmin.ReturnUrl))
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("Login", "AdminLogin");
            }
            return View(accountAdmin);
        }

        [HttpGet]
        [Route("/admin-logout.html", Name = "Admin Logout")]
        public IActionResult Logout()
        {
            HttpContext.SignOutAsync();
            HttpContext.Session.Remove("AccountId");
            return RedirectToAction("Login", "AdminLogin");
        }
    }
}