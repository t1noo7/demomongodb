using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using DemoMongoDB.Models;
using DemoMongoDB.Services;
using AspNetCoreHero.ToastNotification;
using DemoMongoDB.Models.Momo;
using DemoMongoDB.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddNotyf(config => { config.DurationInSeconds = 3; config.IsDismissable = true; config.Position = NotyfPosition.TopRight; });
builder.Configuration.AddJsonFile("appsettings.json");
builder.Services.Configure<MomoOptionModel>(builder.Configuration.GetSection("MomoAPI"));
builder.Services.AddScoped<IMomoService, MomoService>();
builder.Services.AddSingleton<IMongoClient>(serviceProvider =>
    {
        var connectionString = "mongodb+srv://demovscodemongodb:FIahPWMjS2QLt8P6@demomongodb.yhofc7g.mongodb.net/";
        return new MongoClient(connectionString);
    });
builder.Services.AddSession();
builder.Services.AddAuthentication(options =>
<<<<<<< HEAD
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = FacebookDefaults.AuthenticationScheme;
            })
            .AddCookie(p =>
                {
                    p.LoginPath = ("/login.html");
                    p.AccessDeniedPath = ("/");
                })
            .AddCookie("AdminAuth", opt =>
                {
                    opt.LoginPath = "/admin-login.html";
                    opt.AccessDeniedPath = "/admin";
                })
            .AddCookie("StaffAuth", opt =>
                {
                    opt.LoginPath = "/admin-login.html";
                    opt.AccessDeniedPath = "/staff";
                })
=======
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;  // Default scheme for users
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(p => // User login
{
    p.LoginPath = "/login.html";
    p.AccessDeniedPath = "/";
})
.AddCookie("AdminAuthen", p =>  // Admin login
{
    p.LoginPath = "/admin-login.html";
    p.AccessDeniedPath = "/admin";
})
.AddCookie("StaffAuthen", p =>  // Staff login
{
    p.LoginPath = "/admin-login.html";
    p.AccessDeniedPath = "/admin";
})
>>>>>>> refs/remotes/origin/main
            .AddFacebook(options =>
                {
                    options.AppId = builder.Configuration.GetSection("Authentication:Facebook:AppId").Value;
                    options.AppSecret = builder.Configuration.GetSection("Authentication:Facebook:AppSecret").Value;
                    options.CallbackPath = new PathString("/Accounts/FacebookCallback");
                });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminPolicy", policy =>
    {
        policy.RequireRole("Admin");
        policy.RequireAssertion(context => context.User.IsInRole("Admin"));
    });
    options.AddPolicy("StaffPolicy", policy =>
    {
        policy.RequireRole("Staff");
        policy.RequireAssertion(context => context.User.IsInRole("Staff"));
    });
    options.AddPolicy("AdminAndStaffPolicy", policy =>
    {
        policy.RequireRole("Admin", "Staff");
        policy.RequireAssertion(context => context.User.IsInRole("Admin") || context.User.IsInRole("Staff"));
    });
});
<<<<<<< HEAD
=======

>>>>>>> refs/remotes/origin/main

builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddSingleton<IEmailSender, EmailSender>();
builder.Services.AddScoped<IVnPayService, VnPayService>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSession();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
// app.UseNotyf();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
