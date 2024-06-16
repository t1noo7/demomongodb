using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using DemoMongoDB.Models;
using MongoDB.Driver;
using DemoMongoDB.ModelViews;

namespace DemoMongoDB.Controllers;

public class MyCourseController : Controller
{
    private readonly IMongoClient _client;

    public MyCourseController(IMongoClient client)
    {
        _client = client;
    }

    public IActionResult Index()
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
                var myCourseCollection = database.GetCollection<MyCourse>("MyCourse");
                var customer = cusCollection.Find(x => x._id == accountID).SingleOrDefault();
                List<MyCourse> myCourse = myCourseCollection.Find(_ => true).ToList();

                ViewData["yourName"] = customer.FullName;
                ViewData["yourCourse"] = myCourse;
                if (customer == null)
                {
                    return RedirectToAction("Login", "Accounts");
                }
                return View(myCourse);
            }
        }
        catch
        {
            return RedirectToAction("Dashboard", "Accounts");
        }
        return RedirectToAction("Dashboard", "Accounts");
    }
}