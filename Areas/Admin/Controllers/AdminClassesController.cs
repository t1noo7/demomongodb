using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
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
    public class AdminClassesController : Controller
    {
        private readonly IMongoClient _client;

        public AdminClassesController(IMongoClient client)
        {
            _client = client;
        }

        public ActionResult Index(int? page, string Course = null)
        {
            var database = _client.GetDatabase("DemoMongoDb");
            var classesCollection = database.GetCollection<Classes>("Classes");
            var coursesCollection = database.GetCollection<Courses>("Courses");
            var lsCourse = coursesCollection.Find(_ => true).ToList();
            var classessQuery = classesCollection.AsQueryable();

            if (!string.IsNullOrEmpty(Course) && Course != "0")
            {
                var course = coursesCollection.Find(c => c._id == Course).FirstOrDefault();
                if (course != null)
                {
                    classessQuery = classessQuery.Where(c => c.Course == course.Title);
                }
            }

            var pageNumber = page == null || page <= 0 ? 1 : page.Value;
            var pageSize = 20;
            var pagedClasses = classessQuery.ToPagedList(pageNumber, pageSize);

            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = pagedClasses.PageCount;
            ViewData["lsCourse"] = new SelectList(lsCourse, "_id", "Title", Course);

            return View(pagedClasses);
        }

        public IActionResult CoursesFilter(string Course = null)
        {
            var url = $"/Admin/AdminClasses/Index?Course={Course}";
            if (string.IsNullOrEmpty(Course) || Course == "0")
            {
                url = $"/Admin/AdminClasses/Index";
            }
            return Json(new { status = "success", redirectUrl = url });
        }

        public async Task<ActionResult> Create()
        {
            var database = _client.GetDatabase("DemoMongoDb");
            var coursesCollection = database.GetCollection<Courses>("Courses");
            var courses = await coursesCollection.Find(_ => true).ToListAsync();

            // var coursesSelectList = new SelectList(courses, "Title");

            ViewBag.Courses = courses;
            return View();
        }

        // POST: Admin/Adminclasses/Create/5
        [HttpPost]
        public async Task<ActionResult> Create(Classes classes, string idCourse)
        {
            var database = _client.GetDatabase("DemoMongoDb");
            var classesCollection = database.GetCollection<Classes>("Classes");
            var coursesCollection = database.GetCollection<Courses>("Courses");

            classes.CreateDate = DateTime.Now;

            var course = coursesCollection.Find(c => c._id == idCourse).FirstOrDefault();
            if (course != null)
            {
                classes.Course = course.Title;
            }

            classes._id = null; // Assuming MongoDB generates the ID
            await classesCollection.InsertOneAsync(classes);
            return RedirectToAction("Index");
        }




        public ActionResult Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("classes ID is required.");
            }

            if (!ObjectId.TryParse(id, out ObjectId objectId))
            {
                return BadRequest("Invalid classes ID format.");
            }

            var database = _client.GetDatabase("DemoMongoDb");
            var classesCollection = database.GetCollection<Classes>("Classes");

            var filter = Builders<Classes>.Filter.Eq("_id", objectId);
            var classes = classesCollection.Find(filter).FirstOrDefault();

            if (classes == null)
            {
                return NotFound();
            }

            return View(classes);
        }


        public ActionResult Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("classes ID is required.");
            }

            var database = _client.GetDatabase("DemoMongoDb");
            var classesCollection = database.GetCollection<Classes>("Classes");

            var classes = classesCollection.Find(n => n._id == id).FirstOrDefault();

            if (classes == null)
            {
                return NotFound();
            }

            return View(classes);
        }

        // POST: Admin/Adminclasses/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(string id, Classes classes)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("classes ID is required.");
            }

            if (!ModelState.IsValid)
            {
                return View(classes);
            }

            var database = _client.GetDatabase("DemoMongoDb");
            var classesCollection = database.GetCollection<Classes>("Classes");

            var filter = Builders<Classes>.Filter.Eq("_id", id);

            var update = Builders<Classes>.Update
                .Set("Title", classes.Title)
                .Set("Description", classes.Description)
                .Set("Active", classes.Active);

            var result = classesCollection.UpdateOne(filter, update);
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
                return BadRequest("classes ID is required.");
            }

            if (!ObjectId.TryParse(id, out ObjectId objectId))
            {
                return BadRequest("Invalid classes ID format.");
            }

            var database = _client.GetDatabase("DemoMongoDb");
            var classesCollection = database.GetCollection<Classes>("Classes");

            var filter = Builders<Classes>.Filter.Eq("_id", objectId);
            var classes = classesCollection.Find(filter).FirstOrDefault();

            if (classes == null)
            {
                return NotFound();
            }

            return View(classes);
        }

        // POST: Admin/Adminclasses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("classes ID is required.");
            }

            var database = _client.GetDatabase("DemoMongoDb");
            var classesCollection = database.GetCollection<Classes>("Classes");

            var result = classesCollection.DeleteOne(n => n._id == id);
            if (result.DeletedCount == 0)
            {
                return NotFound();
            }

            return RedirectToAction("Index");
        }
    }
}
