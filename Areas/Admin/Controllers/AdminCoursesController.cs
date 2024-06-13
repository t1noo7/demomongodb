using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using DemoMongoDB.Models;
using PagedList.Core;
using DemoMongoDB.Helper;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DemoMongoDB.Controllers
{
    [Area("Admin")]
    public class AdminCoursesController : Controller
    {
        private readonly IMongoClient _client;

        public AdminCoursesController(IMongoClient client)
        {
            _client = client;
        }

        public ActionResult Index(int? page)
        {

            var database = _client.GetDatabase("DemoMongoDb");
            var coursesCollection = database.GetCollection<Courses>("Courses");
            var coursessQuery = coursesCollection.AsQueryable();
            List<Courses> coursessDetails = coursesCollection.Find(_ => true).ToList();
            var pageNumber = page == null || page <= 0 ? 1 : page.Value;
            var pageSize = 20;
            //PagedList<courses> models = new PagedList<courses>(coursesDetails, pageNumber, pageSize);
            var pagedcourses = coursessQuery.ToPagedList(pageNumber, pageSize);
            ViewBag.CurrentPage = pageNumber;
            return View(pagedcourses);
        }
        // public ActionResult Index(int? page)
        // {
        //     var database = _client.GetDatabase("DemoMongoDb");
        //     var coursesCollection = database.GetCollection<courses>("courses");
        //     var coursesQuery = coursesCollection.AsQueryable();
        //     var pageSize = 20;
        //     var pageNumber = page ?? 1;
        //     var skipAmount = (pageNumber - 1) * pageSize;

        //     // Fetch a page of courses items
        //     var pagedcourses = coursesQuery.Skip(skipAmount).Take(pageSize).ToList();

        //     ViewBag.CurrentPage = pageNumber;
        //     ViewBag.TotalPages = (int)Math.Ceiling(coursesQuery.Count() / (double)pageSize);

        //     return View(pagedcourses);
        // }


        public async Task<ActionResult> Create()
        {
            var database = _client.GetDatabase("DemoMongoDb");
            var classesCollection = database.GetCollection<Classes>("Classes");

            var classes = await classesCollection.Find(_ => true).ToListAsync();

            var classSelectList = new SelectList(classes, "_id", "Title");

            ViewBag.Classes = classSelectList;

            return View();
        }

        // POST: Admin/Admincourses/Create/5
        [HttpPost]
        public async Task<ActionResult> Create(Courses courses)
        {
            var database = _client.GetDatabase("DemoMongoDb");
            var coursesCollection = database.GetCollection<Courses>("Courses");
            if (!string.IsNullOrEmpty(courses.YouTubeUrl))
            {
                var videoId = ExtractYouTubeVideoId(courses.YouTubeUrl);
                courses.YouTubeVideoId = videoId;
                courses.Thumb = $"https://img.youtube.com/vi/{videoId}/hqdefault.jpg";
            }
            courses.CreateDate = DateTime.Now;
            courses._id = null;
            coursesCollection.InsertOne(courses);
            return RedirectToAction("Index");
        }



        public ActionResult Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("courses ID is required.");
            }

            if (!ObjectId.TryParse(id, out ObjectId objectId))
            {
                return BadRequest("Invalid courses ID format.");
            }

            var database = _client.GetDatabase("DemoMongoDb");
            var coursesCollection = database.GetCollection<Courses>("Courses");

            var filter = Builders<Courses>.Filter.Eq("_id", objectId);
            var courses = coursesCollection.Find(filter).FirstOrDefault();

            if (courses == null)
            {
                return NotFound();
            }

            return View(courses);
        }


        public async Task<ActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("courses ID is required.");
            }

            var database = _client.GetDatabase("DemoMongoDb");
            var coursesCollection = database.GetCollection<Courses>("Courses");
            var classesCollection = database.GetCollection<Classes>("Classes");
            var classes = await classesCollection.Find(_ => true).ToListAsync();

            var classSelectList = new SelectList(classes, "_id", "Title");

            ViewBag.Classes = classSelectList;

            var courses = coursesCollection.Find(n => n._id == id).FirstOrDefault();

            if (courses == null)
            {
                return NotFound();
            }

            return View(courses);
        }

        // POST: Admin/Admincourses/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(string id, Courses courses)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("courses ID is required.");
            }

            var database = _client.GetDatabase("DemoMongoDb");
            var coursesCollection = database.GetCollection<Courses>("Courses");

            var filter = Builders<Courses>.Filter.Eq("_id", id);
            if (!string.IsNullOrEmpty(courses.YouTubeUrl))
            {
                var videoId = ExtractYouTubeVideoId(courses.YouTubeUrl);
                courses.YouTubeVideoId = videoId;
                courses.Thumb = $"https://img.youtube.com/vi/{videoId}/hqdefault.jpg";
            }

            var update = Builders<Courses>.Update
                .Set("Title", courses.Title)
                .Set("Description", courses.Description)
                .Set("Class", courses.Class)
                .Set("YouTubeUrl", courses.YouTubeUrl)
                .Set("YouTubeVideoId", courses.YouTubeVideoId)
                .Set("Thumb", courses.Thumb)
                .Set("Active", courses.Active);

            var result = coursesCollection.UpdateOne(filter, update);
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
                return BadRequest("courses ID is required.");
            }

            if (!ObjectId.TryParse(id, out ObjectId objectId))
            {
                return BadRequest("Invalid courses ID format.");
            }

            var database = _client.GetDatabase("DemoMongoDb");
            var coursesCollection = database.GetCollection<Courses>("Courses");

            var filter = Builders<Courses>.Filter.Eq("_id", objectId);
            var courses = coursesCollection.Find(filter).FirstOrDefault();

            if (courses == null)
            {
                return NotFound();
            }

            return View(courses);
        }

        // POST: Admin/Admincourses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("courses ID is required.");
            }

            var database = _client.GetDatabase("DemoMongoDb");
            var coursesCollection = database.GetCollection<Courses>("Courses");

            var result = coursesCollection.DeleteOne(n => n._id == id);
            if (result.DeletedCount == 0)
            {
                return NotFound();
            }

            return RedirectToAction("Index");
        }

        private string ExtractYouTubeVideoId(string url)
        {
            var uri = new Uri(url);
            var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
            return query["v"];
        }
    }
}
