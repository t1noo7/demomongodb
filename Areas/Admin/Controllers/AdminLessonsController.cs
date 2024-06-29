using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using DemoMongoDB.Models;
using PagedList.Core;
using DemoMongoDB.Helper;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;

namespace DemoMongoDB.Controllers
{
<<<<<<< HEAD
    [Authorize(Roles = "Admin, Staff", Policy = "AdminAndStaffPolicy", AuthenticationSchemes = "AdminAuth, StaffAuth")]
=======
    [Authorize(Roles = "Admin, Staff", Policy = "AdminAndStaffPolicy", AuthenticationSchemes = "AdminAuthen, StaffAuthen")]
>>>>>>> refs/remotes/origin/main
    [Area("Admin")]
    public class AdminLessonsController : Controller
    {
        private readonly IMongoClient _client;

        public AdminLessonsController(IMongoClient client)
        {
            _client = client;
        }

        public ActionResult Index(int? page)
        {

            var database = _client.GetDatabase("DemoMongoDb");
            var lessonsCollection = database.GetCollection<Lessons>("Lessons");
            var lessonssQuery = lessonsCollection.AsQueryable();
            List<Lessons> lessonssDetails = lessonsCollection.Find(_ => true).ToList();
            var pageNumber = page == null || page <= 0 ? 1 : page.Value;
            var pageSize = 20;
            //PagedList<lessons> models = new PagedList<lessons>(lessonsDetails, pageNumber, pageSize);
            var pagedLessons = lessonssQuery.ToPagedList(pageNumber, pageSize);
            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = pagedLessons.PageCount;
            return View(pagedLessons);
        }
        // public ActionResult Index(int? page)
        // {
        //     var database = _client.GetDatabase("DemoMongoDb");
        //     var lessonsCollection = database.GetCollection<lessons>("lessons");
        //     var lessonsQuery = lessonsCollection.AsQueryable();
        //     var pageSize = 20;
        //     var pageNumber = page ?? 1;
        //     var skipAmount = (pageNumber - 1) * pageSize;

        //     // Fetch a page of lessons items
        //     var pagedlessons = lessonsQuery.Skip(skipAmount).Take(pageSize).ToList();

        //     ViewBag.CurrentPage = pageNumber;
        //     ViewBag.TotalPages = (int)Math.Ceiling(lessonsQuery.Count() / (double)pageSize);

        //     return View(pagedlessons);
        // }


        public async Task<ActionResult> Create()
        {
            var database = _client.GetDatabase("DemoMongoDb");
            var courseCollection = database.GetCollection<Courses>("Courses");

            var course = await courseCollection.Find(_ => true).ToListAsync();

            var courseSelectList = new SelectList(course, "Title");

            ViewBag.lsCourse = courseSelectList;

            return View();
        }

        // POST: Admin/Adminlessons/Create/5
        [HttpPost]
        public async Task<ActionResult> Create(Lessons lessons)
        {
            var database = _client.GetDatabase("DemoMongoDb");
            var lessonsCollection = database.GetCollection<Lessons>("Lessons");
            if (!string.IsNullOrEmpty(lessons.YouTubeUrl))
            {
                var videoId = ExtractYouTubeVideoId(lessons.YouTubeUrl);
                lessons.YouTubeVideoId = videoId;
                lessons.Thumb = $"https://img.youtube.com/vi/{videoId}/hqdefault.jpg";
            }
            lessons.CreateDate = DateTime.Now;
            lessons._id = null;
            lessonsCollection.InsertOne(lessons);
            return RedirectToAction("Index");
        }



        public ActionResult Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("lessons ID is required.");
            }

            if (!ObjectId.TryParse(id, out ObjectId objectId))
            {
                return BadRequest("Invalid lessons ID format.");
            }

            var database = _client.GetDatabase("DemoMongoDb");
            var lessonsCollection = database.GetCollection<Lessons>("Lessons");

            var filter = Builders<Lessons>.Filter.Eq("_id", objectId);
            var lessons = lessonsCollection.Find(filter).FirstOrDefault();

            if (lessons == null)
            {
                return NotFound();
            }

            return View(lessons);
        }


        public async Task<ActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("lessons ID is required.");
            }

            var database = _client.GetDatabase("DemoMongoDb");
            var lessonsCollection = database.GetCollection<Lessons>("Lessons");
            var courseCollection = database.GetCollection<Courses>("Courses");
            var courses = await courseCollection.Find(_ => true).ToListAsync();

            // var courseSelectList = new SelectList(courses, "Title");

            ViewBag.lsCourse = courses;

            var lessons = lessonsCollection.Find(n => n._id == id).FirstOrDefault();

            if (lessons == null)
            {
                return NotFound();
            }

            return View(lessons);
        }

        // POST: Admin/Adminlessons/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(string id, Lessons lessons, string idCourse)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("lessons ID is required.");
            }

            var database = _client.GetDatabase("DemoMongoDb");
            var lessonsCollection = database.GetCollection<Lessons>("Lessons");
            var coursesCollection = database.GetCollection<Courses>("Courses");

            var filter = Builders<Lessons>.Filter.Eq("_id", id);
            if (!string.IsNullOrEmpty(lessons.YouTubeUrl))
            {
                var videoId = ExtractYouTubeVideoId(lessons.YouTubeUrl);
                lessons.YouTubeVideoId = videoId;
                lessons.Thumb = $"https://img.youtube.com/vi/{videoId}/hqdefault.jpg";
            }

            var course = await coursesCollection.Find(c => c._id == idCourse).FirstOrDefaultAsync();
            if (course != null)
            {
                lessons.Course = course.Title;
            }

            var update = Builders<Lessons>.Update
                .Set("Title", lessons.Title)
                .Set("Description", lessons.Description)
                .Set("Course", lessons.Course)
                .Set("YouTubeUrl", lessons.YouTubeUrl)
                .Set("YouTubeVideoId", lessons.YouTubeVideoId)
                .Set("Thumb", lessons.Thumb)
                .Set("Active", lessons.Active);

            var result = await lessonsCollection.UpdateOneAsync(filter, update);
            if (result.ModifiedCount == 0)
            {
                return RedirectToAction("Index");
            }

            return RedirectToAction("Index");
        }


        public ActionResult Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("lessons ID is required.");
            }

            if (!ObjectId.TryParse(id, out ObjectId objectId))
            {
                return BadRequest("Invalid lessons ID format.");
            }

            var database = _client.GetDatabase("DemoMongoDb");
            var lessonsCollection = database.GetCollection<Lessons>("Lessons");

            var filter = Builders<Lessons>.Filter.Eq("_id", objectId);
            var lessons = lessonsCollection.Find(filter).FirstOrDefault();

            if (lessons == null)
            {
                return NotFound();
            }

            return View(lessons);
        }

        // POST: Admin/Adminlessons/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("lessons ID is required.");
            }

            var database = _client.GetDatabase("DemoMongoDb");
            var lessonsCollection = database.GetCollection<Lessons>("Lessons");

            var result = lessonsCollection.DeleteOne(n => n._id == id);
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
