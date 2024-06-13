using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using DemoMongoDB.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Configuration.AddJsonFile("appsettings.json");
builder.Services.AddSingleton<IMongoClient>(serviceProvider =>
    {
        var connectionString = "mongodb+srv://demovscodemongodb:FIahPWMjS2QLt8P6@demomongodb.yhofc7g.mongodb.net/";
        return new MongoClient(connectionString);
    });
builder.Services.AddSession();
builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = FacebookDefaults.AuthenticationScheme;
            })
            .AddCookie(p =>
                {
                    p.LoginPath = ("/login.html");
                    p.AccessDeniedPath = ("/");
                })
            .AddFacebook(options =>
                {
                    options.AppId = builder.Configuration.GetSection("Authentication:Facebook:AppId").Value;
                    options.AppSecret = builder.Configuration.GetSection("Authentication:Facebook:AppSecret").Value;
                    options.CallbackPath = new PathString("/Accounts/FacebookCallback");
                });


builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddSingleton<IEmailSender, EmailSender>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}else{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSession();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
