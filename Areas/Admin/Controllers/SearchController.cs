using DemoMongoDB.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace DemoMongoDB.Controllers
{
    [Area("Admin")]
    public class SearchController : Controller
    {
        private readonly IMongoClient _client;

        public SearchController(IMongoClient client)
        {
            _client = client;
        }
        // GET: Search/FIndProduct
        [HttpPost]
        public IActionResult FindCourse(string keyword)
        {
            var database = _client.GetDatabase("DemoMongoDb");
            var classesCollection = database.GetCollection<Classes>("Classes");

            List<Classes> lsClass;

            if (string.IsNullOrEmpty(keyword) || keyword.Length < 1)
            {
                lsClass = classesCollection.Find(Builders<Classes>.Filter.Empty).SortBy(x => x.CreateDate).ToList();
            }
            else
            {
                var filter = Builders<Classes>.Filter.Regex("Course", new MongoDB.Bson.BsonRegularExpression(keyword, "i"));
                lsClass = classesCollection.Find(filter).SortBy(x => x.CreateDate).ToList();
            }

            return PartialView("ListProductSearchPartial", lsClass);
        }
    }
}