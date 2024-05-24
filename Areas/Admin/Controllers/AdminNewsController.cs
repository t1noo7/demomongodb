using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using DemoMongoDB.Models;
using PagedList.Core;
using DemoMongoDB.Helper;

namespace DemoMongoDB.Controllers
{
    [Area("Admin")]
    public class AdminNewsController : Controller
    {
        private readonly IMongoClient _client;

        public AdminNewsController(IMongoClient client)
        {
            _client = client;
        }

        public ActionResult Index(int? page)
        {

            var database = _client.GetDatabase("DemoMongoDb");
            var newsCollection = database.GetCollection<News>("News");
            var newsQuery = newsCollection.AsQueryable();
            List<News> newsDetails = newsCollection.Find(_ => true).ToList();
            var pageNumber = page == null || page <= 0 ? 1 : page.Value;
            var pageSize = 20;
            //PagedList<News> models = new PagedList<News>(newsDetails, pageNumber, pageSize);
            var pagedNews = newsQuery.ToPagedList(pageNumber, pageSize);
            ViewBag.CurrentPage = pageNumber;
            return View(pagedNews);
        }
        // public ActionResult Index(int? page)
        // {
        //     var database = _client.GetDatabase("DemoMongoDb");
        //     var newsCollection = database.GetCollection<News>("News");
        //     var newsQuery = newsCollection.AsQueryable();
        //     var pageSize = 20;
        //     var pageNumber = page ?? 1;
        //     var skipAmount = (pageNumber - 1) * pageSize;

        //     // Fetch a page of news items
        //     var pagedNews = newsQuery.Skip(skipAmount).Take(pageSize).ToList();

        //     ViewBag.CurrentPage = pageNumber;
        //     ViewBag.TotalPages = (int)Math.Ceiling(newsQuery.Count() / (double)pageSize);

        //     return View(pagedNews);
        // }


        public ActionResult Create()
        {
            return View();
        }

        // POST: Admin/AdminNews/Create/5
        [HttpPost]
        public async Task<ActionResult> Create(News news, Microsoft.AspNetCore.Http.IFormFile fThumb)
        {
            var database = _client.GetDatabase("DemoMongoDb");
            var newsCollection = database.GetCollection<News>("News");
            if (fThumb != null)
            {
                string extension = Path.GetExtension(fThumb.FileName);
                string image = Utilities.SEOUrl(news.Title) + extension;
                news.Thumb = await Utilities.UploadFile(fThumb, @"news", image.ToLower());
            }
            if (string.IsNullOrEmpty(news.Thumb)) news.Thumb = "default.jpg";
            news.CreateDate = DateTime.Now;
            news.Alias = Utilities.SEOUrl(news.Title);
            news._id = null;
            newsCollection.InsertOne(news);
            return RedirectToAction("Index");
        }



        public ActionResult Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("News ID is required.");
            }

            if (!ObjectId.TryParse(id, out ObjectId objectId))
            {
                return BadRequest("Invalid News ID format.");
            }

            var database = _client.GetDatabase("DemoMongoDb");
            var newsCollection = database.GetCollection<News>("News");

            var filter = Builders<News>.Filter.Eq("_id", objectId);
            var news = newsCollection.Find(filter).FirstOrDefault();

            if (news == null)
            {
                return NotFound();
            }

            return View(news);
        }


        public ActionResult Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("News ID is required.");
            }

            var database = _client.GetDatabase("DemoMongoDb");
            var newsCollection = database.GetCollection<News>("News");

            var news = newsCollection.Find(n => n._id == id).FirstOrDefault();

            if (news == null)
            {
                return NotFound();
            }

            return View(news);
        }

        // POST: Admin/AdminNews/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(string id, News news, Microsoft.AspNetCore.Http.IFormFile fThumb)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("News ID is required.");
            }

            if (!ModelState.IsValid)
            {
                return View(news);
            }

            var database = _client.GetDatabase("DemoMongoDb");
            var newsCollection = database.GetCollection<News>("News");

            var filter = Builders<News>.Filter.Eq("_id", id);
            if (fThumb != null)
            {
                string extension = Path.GetExtension(fThumb.FileName);
                string image = Utilities.SEOUrl(news.Title) + extension;
                news.Thumb = await Utilities.UploadFile(fThumb, @"news", image.ToLower());
            }
            if (string.IsNullOrEmpty(news.Thumb)) news.Thumb = "default.jpg";
            var update = Builders<News>.Update
                .Set("Title", news.Title)
                .Set("Contents", news.Contents)
                .Set("CreateDate", DateTime.Now)
                .Set("Thumb", news.Thumb);

            var result = newsCollection.UpdateOne(filter, update);
            if (result.ModifiedCount == 0)
            {
                return NotFound();
            }

            return RedirectToAction("Index");
        }

        public ActionResult Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("News ID is required.");
            }

            if (!ObjectId.TryParse(id, out ObjectId objectId))
            {
                return BadRequest("Invalid News ID format.");
            }

            var database = _client.GetDatabase("DemoMongoDb");
            var newsCollection = database.GetCollection<News>("News");

            var filter = Builders<News>.Filter.Eq("_id", objectId);
            var news = newsCollection.Find(filter).FirstOrDefault();

            if (news == null)
            {
                return NotFound();
            }

            return View(news);
        }

        // POST: Admin/AdminNews/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("News ID is required.");
            }

            var database = _client.GetDatabase("DemoMongoDb");
            var newsCollection = database.GetCollection<News>("News");

            var result = newsCollection.DeleteOne(n => n._id == id);
            if (result.DeletedCount == 0)
            {
                return NotFound();
            }

            return RedirectToAction("Index");
        }
    }
}
