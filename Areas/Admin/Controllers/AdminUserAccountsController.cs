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
    public class AdminUserAccountsController : Controller
    {
        private readonly IMongoClient _client;

        public AdminUserAccountsController(IMongoClient client)
        {
            _client = client;
        }

        public ActionResult Index(int? page)
        {

            var database = _client.GetDatabase("DemoMongoDb");
            var userAccountsCollection = database.GetCollection<UserAccounts>("UserAccounts");
            var userAccountsQuery = userAccountsCollection.AsQueryable();
            List<UserAccounts> userAccountsDetails = userAccountsCollection.Find(_ => true).ToList();
            var pageNumber = page == null || page <= 0 ? 1 : page.Value;
            var pageSize = 20;
            //PagedList<News> models = new PagedList<News>(newsDetails, pageNumber, pageSize);
            var pagedUserAccounts = userAccountsQuery.ToPagedList(pageNumber, pageSize);
            ViewBag.CurrentPage = pageNumber;
            return View(pagedUserAccounts);
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
        public async Task<ActionResult> Create(UserAccounts userAccounts, Microsoft.AspNetCore.Http.IFormFile fThumb)
        {
            var database = _client.GetDatabase("DemoMongoDb");
            var userAccountsCollection = database.GetCollection<UserAccounts>("UserAccount");
            if (fThumb != null)
            {
                string extension = Path.GetExtension(fThumb.FileName);
                string image = Utilities.SEOUrl(userAccounts.FullName) + extension;
                userAccounts.Avatar = await Utilities.UploadFile(fThumb, @"avatars", image.ToLower());
            }
            if (string.IsNullOrEmpty(userAccounts.Avatar)) userAccounts.Avatar = "default.jpg";
            userAccounts.CreateDate = DateTime.Now;
            userAccounts._id = null;
            userAccountsCollection.InsertOne(userAccounts);
            return RedirectToAction("Index");
        }



        public ActionResult Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("UserAccounts ID is required.");
            }

            if (!ObjectId.TryParse(id, out ObjectId objectId))
            {
                return BadRequest("Invalid UserAccounts ID format.");
            }

            var database = _client.GetDatabase("DemoMongoDb");
            var userAccountsCollection = database.GetCollection<UserAccounts>("UserAccounts");

            var filter = Builders<UserAccounts>.Filter.Eq("_id", objectId);
            var userAccounts = userAccountsCollection.Find(filter).FirstOrDefault();

            if (userAccounts == null)
            {
                return NotFound();
            }

            return View(userAccounts);
        }


        public ActionResult Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("UserAccounts ID is required.");
            }

            var database = _client.GetDatabase("DemoMongoDb");
            var userAccountsCollection = database.GetCollection<UserAccounts>("UserAccounts");

            var userAccounts = userAccountsCollection.Find(n => n._id == id).FirstOrDefault();

            if (userAccounts == null)
            {
                return NotFound();
            }

            return View(userAccounts);
        }

        // POST: Admin/AdminNews/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(string id, UserAccounts userAccounts, Microsoft.AspNetCore.Http.IFormFile fThumb)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("UserAccounts ID is required.");
            }

            if (!ModelState.IsValid)
            {
                return View(userAccounts);
            }

            var database = _client.GetDatabase("DemoMongoDb");
            var userAccountsCollection = database.GetCollection<UserAccounts>("UserAccounts");

            var filter = Builders<UserAccounts>.Filter.Eq("_id", id);
            if (fThumb != null)
            {
                string extension = Path.GetExtension(fThumb.FileName);
                string image = Utilities.SEOUrl(userAccounts.FullName) + extension;
                userAccounts.Avatar = await Utilities.UploadFile(fThumb, @"avatars", image.ToLower());
            }
            if (string.IsNullOrEmpty(userAccounts.Avatar)) userAccounts.Avatar = "default.jpg";
            var update = Builders<UserAccounts>.Update
                .Set("FullName", userAccounts.FullName)
                .Set("Birthday", userAccounts.Birthday)
                .Set("CreateDate", DateTime.Now)
                .Set("Avatar", userAccounts.Avatar)
                .Set("Address", userAccounts.Address)
                .Set("Phone", userAccounts.Phone)
                .Set("Password", userAccounts.Password)
                .Set("LastLogin", userAccounts.LastLogin)
                .Set("Active", userAccounts.Active)
                .Set("Email", userAccounts.Email);

            var result = userAccountsCollection.UpdateOne(filter, update);
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
                return BadRequest("UserAccounts ID is required.");
            }

            if (!ObjectId.TryParse(id, out ObjectId objectId))
            {
                return BadRequest("Invalid UserAccounts ID format.");
            }

            var database = _client.GetDatabase("DemoMongoDb");
            var userAccountsCollection = database.GetCollection<UserAccounts>("UserAccounts");

            var filter = Builders<UserAccounts>.Filter.Eq("_id", objectId);
            var userAccounts = userAccountsCollection.Find(filter).FirstOrDefault();

            if (userAccounts == null)
            {
                return NotFound();
            }

            return View(userAccounts);
        }

        // POST: Admin/AdminNews/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("UserAccounts ID is required.");
            }

            var database = _client.GetDatabase("DemoMongoDb");
            var userAccountsCollection = database.GetCollection<UserAccounts>("UserAccounts");

            var result = userAccountsCollection.DeleteOne(n => n._id == id);
            if (result.DeletedCount == 0)
            {
                return NotFound();
            }

            return RedirectToAction("Index");
        }
    }
}
