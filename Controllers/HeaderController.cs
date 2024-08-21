using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using DemoMongoDB.Models;
using MongoDB.Driver;
using DemoMongoDB.ModelViews;

namespace DemoMongoDB.Controllers;

public class HeaderController : Controller
{
    private readonly IMongoClient _client;

    public HeaderController(IMongoClient client)
    {
        _client = client;
    }

    public IActionResult _HeaderPartialView()
    {
        var database = _client.GetDatabase("DemoMongoDb");
        var CategoriesCollection = database.GetCollection<Categories>("Categories");
        List<Categories> CategoriesDetails = CategoriesCollection.Find(_ => true).ToList();

        // Lọc và chỉ chọn các SubCat có IsActive bằng true
        var activeSubCats = CategoriesDetails
            .SelectMany(c => c.SubCat) // Lấy tất cả các SubCat từ danh sách Categories
            .Where(sc => sc.IsActive) // Chỉ lấy những SubCat có IsActive là true
            .ToList();

        ViewBag.Categories = CategoriesDetails;
        ViewBag.ActiveSubCats = activeSubCats;

        return PartialView("_HeaderPartialView");
    }



}
