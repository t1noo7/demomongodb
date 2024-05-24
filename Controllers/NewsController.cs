using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using DemoMongoDB.Models;
using MongoDB.Driver;
using DemoMongoDB.ModelViews;

namespace DemoMongoDB.Controllers;

public class NewsController : Controller
{
    private readonly IMongoClient _client;

    public NewsController(IMongoClient client)
    {
        _client = client;
    }


    [Route("/news/{Alias}", Name = "News Detail")]

    public IActionResult Details(string Alias)
    {
        var database = _client.GetDatabase("DemoMongoDb");
        var NewsCollection = database.GetCollection<News>("News");
        if (string.IsNullOrEmpty(Alias)) return RedirectToAction("Home", "Index");
        var NewsDetails = NewsCollection.Find(x => x.Alias == Alias).SingleOrDefault();
        if (NewsDetails == null)
        {
            return RedirectToAction("Index", "Home");
        }
        return View(NewsDetails);
    }


}
