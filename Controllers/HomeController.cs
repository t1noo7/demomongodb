using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using DemoMongoDB.Models;
using MongoDB.Driver;
using DemoMongoDB.ModelViews;

namespace DemoMongoDB.Controllers;

public class HomeController : Controller
{
    private readonly IMongoClient _client;

    public HomeController(IMongoClient client)
    {
        _client = client;
    }

    public IActionResult Index()
    {
        HomeViewVM model = new HomeViewVM();

        var database = _client.GetDatabase("DemoMongoDb");
        var BannersCollection = database.GetCollection<Banners>("Banners");
        var NewsCollection = database.GetCollection<News>("News");
        var CategoriesCollection = database.GetCollection<Categories>("Categories");

        List<Banners> BannersDetails = BannersCollection.Find(x => x.Active == true).SortBy(x => x.OrderIndex).ToList();
        List<News> NewsDetails = NewsCollection.Find(_ => true).ToList();
        List<Categories> CategoriesDetails = CategoriesCollection.Find(_ => true).ToList();

        model.Banners = BannersDetails;
        model.News = NewsDetails;
        model.Categories = CategoriesDetails;

        return View(model);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
