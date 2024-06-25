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
    public class AdminRolesController : Controller
    {
        private readonly IMongoClient _client;

        public AdminRolesController(IMongoClient client)
        {
            _client = client;
        }

        public ActionResult Index(int? page)
        {

            var database = _client.GetDatabase("DemoMongoDb");
            var rolesCollection = database.GetCollection<Roles>("Roles");
            var rolesQuery = rolesCollection.AsQueryable();
            List<Roles> rolesDetails = rolesCollection.Find(_ => true).ToList();
            var pageNumber = page == null || page <= 0 ? 1 : page.Value;
            var pageSize = 20;
            //PagedList<roles> models = new PagedList<roles>(rolesDetails, pageNumber, pageSize);
            var pagedroles = rolesQuery.ToPagedList(pageNumber, pageSize);
            ViewBag.CurrentPage = pageNumber;
            return View(pagedroles);
        }
        // public ActionResult Index(int? page)
        // {
        //     var database = _client.GetDatabase("DemoMongoDb");
        //     var rolesCollection = database.GetCollection<roles>("roles");
        //     var rolesQuery = rolesCollection.AsQueryable();
        //     var pageSize = 20;
        //     var pageNumber = page ?? 1;
        //     var skipAmount = (pageNumber - 1) * pageSize;

        //     // Fetch a page of roles items
        //     var pagedroles = rolesQuery.Skip(skipAmount).Take(pageSize).ToList();

        //     ViewBag.CurrentPage = pageNumber;
        //     ViewBag.TotalPages = (int)Math.Ceiling(rolesQuery.Count() / (double)pageSize);

        //     return View(pagedroles);
        // }


        public ActionResult Create()
        {
            return View();
        }

        // POST: Admin/Adminroles/Create/5
        [HttpPost]
        public async Task<ActionResult> Create(Roles roles)
        {
            var database = _client.GetDatabase("DemoMongoDb");
            var rolesCollection = database.GetCollection<Roles>("Roles");
            roles._id = null;
            rolesCollection.InsertOne(roles);
            return RedirectToAction("Index");
        }



        public ActionResult Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("roles ID is required.");
            }

            if (!ObjectId.TryParse(id, out ObjectId objectId))
            {
                return BadRequest("Invalid roles ID format.");
            }

            var database = _client.GetDatabase("DemoMongoDb");
            var rolesCollection = database.GetCollection<Roles>("Roles");

            var filter = Builders<Roles>.Filter.Eq("_id", objectId);
            var roles = rolesCollection.Find(filter).FirstOrDefault();

            if (roles == null)
            {
                return NotFound();
            }

            return View(roles);
        }


        public ActionResult Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("Roles ID is required.");
            }

            var database = _client.GetDatabase("DemoMongoDb");
            var rolesCollection = database.GetCollection<Roles>("Roles");

            var roles = rolesCollection.Find(n => n._id == id).FirstOrDefault();

            if (roles == null)
            {
                return NotFound();
            }

            return View(roles);
        }

        // POST: Admin/Adminroles/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(string id, Roles roles)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("Roles ID is required.");
            }

            if (!ModelState.IsValid)
            {
                return View(roles);
            }

            var database = _client.GetDatabase("DemoMongoDb");
            var rolesCollection = database.GetCollection<Roles>("Roles");

            var filter = Builders<Roles>.Filter.Eq("_id", id);
            var update = Builders<Roles>.Update
                .Set("RoleName", roles.RoleName)
                .Set("Description", roles.Description);

            var result = rolesCollection.UpdateOne(filter, update);
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
                return BadRequest("Roles ID is required.");
            }

            if (!ObjectId.TryParse(id, out ObjectId objectId))
            {
                return BadRequest("Invalid roles ID format.");
            }

            var database = _client.GetDatabase("DemoMongoDb");
            var rolesCollection = database.GetCollection<Roles>("Roles");

            var filter = Builders<Roles>.Filter.Eq("_id", objectId);
            var roles = rolesCollection.Find(filter).FirstOrDefault();

            if (roles == null)
            {
                return NotFound();
            }

            return View(roles);
        }

        // POST: Admin/Adminroles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("roles ID is required.");
            }

            var database = _client.GetDatabase("DemoMongoDb");
            var rolesCollection = database.GetCollection<Roles>("Roles");

            var result = rolesCollection.DeleteOne(n => n._id == id);
            if (result.DeletedCount == 0)
            {
                return NotFound();
            }

            return RedirectToAction("Index");
        }
    }
}
