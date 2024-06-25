using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using DemoMongoDB.Models;
using PagedList.Core;
using DemoMongoDB.Helper;
using Microsoft.AspNetCore.Authorization;

namespace DemoMongoDB.Controllers
{
    [Authorize(Roles = "Admin, Staff", Policy = "AdminAndStaffPolicy", AuthenticationSchemes = "AdminAuthen, StaffAuthen")]
    [Area("Admin")]
    public class AdminCategoriesController : Controller
    {
        private readonly IMongoClient _client;

        public AdminCategoriesController(IMongoClient client)
        {
            _client = client;
        }

        public ActionResult Index(int? page)
        {

            var database = _client.GetDatabase("DemoMongoDb");
            var CategoriesCollection = database.GetCollection<Categories>("Categories");
            var CategoriesQuery = CategoriesCollection.AsQueryable();
            List<Categories> CategoriesDetails = CategoriesCollection.Find(_ => true).ToList();
            var pageNumber = page == null || page <= 0 ? 1 : page.Value;
            var pageSize = 20;
            //PagedList<Categories> models = new PagedList<Categories>(CategoriesDetails, pageNumber, pageSize);
            var pagedCategories = CategoriesQuery.ToPagedList(pageNumber, pageSize);
            ViewBag.CurrentPage = pageNumber;
            return View(pagedCategories);
        }
        // public ActionResult Index(int? page)
        // {
        //     var database = _client.GetDatabase("DemoMongoDb");
        //     var CategoriesCollection = database.GetCollection<Categories>("Categories");
        //     var CategoriesQuery = CategoriesCollection.AsQueryable();
        //     var pageSize = 20;
        //     var pageNumber = page ?? 1;
        //     var skipAmount = (pageNumber - 1) * pageSize;

        //     // Fetch a page of Categories items
        //     var pagedCategories = CategoriesQuery.Skip(skipAmount).Take(pageSize).ToList();

        //     ViewBag.CurrentPage = pageNumber;
        //     ViewBag.TotalPages = (int)Math.Ceiling(CategoriesQuery.Count() / (double)pageSize);

        //     return View(pagedCategories);
        // }


        public ActionResult Create()
        {
            return View();
        }

        // POST: Admin/AdminCategories/Create/5
        [HttpPost]
        public async Task<ActionResult> Create(Categories Categories, Microsoft.AspNetCore.Http.IFormFile fThumb, List<SubCat> SubCat)
        {
            var database = _client.GetDatabase("DemoMongoDb");
            var CategoriesCollection = database.GetCollection<Categories>("Categories");

            if (fThumb != null)
            {
                string extension = Path.GetExtension(fThumb.FileName);
                string image = Utilities.SEOUrl(Categories.Title) + extension;
                Categories.Thumb = await Utilities.UploadFile(fThumb, @"categories", image.ToLower());
            }
            if (string.IsNullOrEmpty(Categories.Thumb)) Categories.Thumb = "default.jpg";

            Categories._id = null;

            // Debugging: Log the SubCat list
            foreach (var subCat in SubCat)
            {
                Console.WriteLine($"SubCat Name: {subCat.Name}, IsActive: {subCat.IsActive}");
            }

            Categories.SubCat = SubCat;

            CategoriesCollection.InsertOne(Categories);
            return RedirectToAction("Index");
        }


        public ActionResult Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("Categories ID is required.");
            }

            if (!ObjectId.TryParse(id, out ObjectId objectId))
            {
                return BadRequest("Invalid Categories ID format.");
            }

            var database = _client.GetDatabase("DemoMongoDb");
            var CategoriesCollection = database.GetCollection<Categories>("Categories");

            var filter = Builders<Categories>.Filter.Eq("_id", objectId);
            var Categories = CategoriesCollection.Find(filter).FirstOrDefault();

            if (Categories == null)
            {
                return NotFound();
            }

            return View(Categories);
        }


        // GET: Admin/AdminCategories/Edit/5
        public ActionResult Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("Categories ID is required.");
            }

            var database = _client.GetDatabase("DemoMongoDb");
            var CategoriesCollection = database.GetCollection<Categories>("Categories");

            var category = CategoriesCollection.Find(n => n._id == id).FirstOrDefault();

            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // POST: Admin/AdminCategories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(string id, Categories model)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("Categories ID is required.");
            }

            var database = _client.GetDatabase("DemoMongoDb");
            var CategoriesCollection = database.GetCollection<Categories>("Categories");

            var filter = Builders<Categories>.Filter.Eq("_id", id);

            // Update SubCat.IsActive based on checkbox state
            foreach (var subCat in model.SubCat)
            {
                subCat.IsActive = subCat.IsActive; // Ensure IsActive property is correctly bound
            }

            var update = Builders<Categories>.Update
                .Set("Title", model.Title)
                .Set("CatName", model.CatName)
                .Set("Description", model.Description)
                .Set("Published", model.Published)
                .Set("Thumb", model.Thumb)
                .Set("SubCat", model.SubCat);

            var result = CategoriesCollection.UpdateOne(filter, update);
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
                return BadRequest("Categories ID is required.");
            }

            if (!ObjectId.TryParse(id, out ObjectId objectId))
            {
                return BadRequest("Invalid Categories ID format.");
            }

            var database = _client.GetDatabase("DemoMongoDb");
            var CategoriesCollection = database.GetCollection<Categories>("Categories");

            var filter = Builders<Categories>.Filter.Eq("_id", objectId);
            var Categories = CategoriesCollection.Find(filter).FirstOrDefault();

            if (Categories == null)
            {
                return NotFound();
            }

            return View(Categories);
        }

        // POST: Admin/AdminCategories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("Categories ID is required.");
            }

            var database = _client.GetDatabase("DemoMongoDb");
            var CategoriesCollection = database.GetCollection<Categories>("Categories");

            var result = CategoriesCollection.DeleteOne(n => n._id == id);
            if (result.DeletedCount == 0)
            {
                return NotFound();
            }

            return RedirectToAction("Index");
        }

    }
}
