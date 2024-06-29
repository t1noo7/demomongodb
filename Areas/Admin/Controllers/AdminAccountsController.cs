using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using DemoMongoDB.Models;
using PagedList.Core;
using DemoMongoDB.Helper;
using DemoMongoDB.Extension;
using Microsoft.AspNetCore.Authorization;

namespace DemoMongoDB.Controllers
{
    [Authorize(Roles = "Admin, Staff", Policy = "AdminAndStaffPolicy", AuthenticationSchemes = "AdminAuth, StaffAuth")]
    [Area("Admin")]
    public class AdminAccountsController : Controller
    {
        private readonly IMongoClient _client;

        public AdminAccountsController(IMongoClient client)
        {
            _client = client;
        }

        public ActionResult Index(int? page)
        {

            var database = _client.GetDatabase("DemoMongoDb");
            var adminAccountsCollection = database.GetCollection<AdminAccounts>("AdminAccounts");
            var adminAccountsQuery = adminAccountsCollection.AsQueryable();
            List<AdminAccounts> adminAccountsDetails = adminAccountsCollection.Find(_ => true).ToList();
            var pageNumber = page == null || page <= 0 ? 1 : page.Value;
            var pageSize = 20;
            //PagedList<adminAccounts> models = new PagedList<adminAccounts>(adminAccountsDetails, pageNumber, pageSize);
            var pagedadminAccounts = adminAccountsQuery.ToPagedList(pageNumber, pageSize);
            ViewBag.CurrentPage = pageNumber;
            return View(pagedadminAccounts);
        }
        // public ActionResult Index(int? page)
        // {
        //     var database = _client.GetDatabase("DemoMongoDb");
        //     var adminAccountsCollection = database.GetCollection<adminAccounts>("adminAccounts");
        //     var adminAccountsQuery = adminAccountsCollection.AsQueryable();
        //     var pageSize = 20;
        //     var pageNumber = page ?? 1;
        //     var skipAmount = (pageNumber - 1) * pageSize;

        //     // Fetch a page of adminAccounts items
        //     var pagedadminAccounts = adminAccountsQuery.Skip(skipAmount).Take(pageSize).ToList();

        //     ViewBag.CurrentPage = pageNumber;
        //     ViewBag.TotalPages = (int)Math.Ceiling(adminAccountsQuery.Count() / (double)pageSize);

        //     return View(pagedadminAccounts);
        // }


        public ActionResult Create()
        {
            var database = _client.GetDatabase("DemoMongoDb");
            var rolesCollection = database.GetCollection<Roles>("Roles");

            var roles = rolesCollection.Find(_ => true).ToList();

            ViewBag.Roles = roles;
            return View();
        }

        // POST: Admin/AdminadminAccounts/Create/5
        [HttpPost]
        public async Task<ActionResult> Create(AdminAccounts adminAccounts, string idRoles)
        {
            var database = _client.GetDatabase("DemoMongoDb");
            var adminAccountsCollection = database.GetCollection<AdminAccounts>("AdminAccounts");
            var rolesCollection = database.GetCollection<Roles>("Roles");

            var roleName = rolesCollection.Find(r => r._id == idRoles).FirstOrDefault();
            if (roleName != null)
            {
                adminAccounts.Role = roleName.RoleName;
            }

            string salt = Utilities.GetRandomKey();
            adminAccounts.CreateDate = DateTime.Now;
            adminAccounts.RoleId = idRoles;
            adminAccounts.Salt = salt;
            adminAccounts.Password = (adminAccounts.Password + salt.Trim()).ToMD5();
            adminAccounts._id = null;
            adminAccountsCollection.InsertOne(adminAccounts);
            return RedirectToAction("Index");
        }



        // public ActionResult Details(string id)
        // {
        //     if (string.IsNullOrEmpty(id))
        //     {
        //         return BadRequest("AdminAccounts ID is required.");
        //     }

        //     if (!ObjectId.TryParse(id, out ObjectId objectId))
        //     {
        //         return BadRequest("Invalid AdminAccounts ID format.");
        //     }

        //     var database = _client.GetDatabase("DemoMongoDb");
        //     var adminAccountsCollection = database.GetCollection<AdminAccounts>("AdminAccounts");

        //     var filter = Builders<AdminAccounts>.Filter.Eq("_id", objectId);
        //     var adminAccounts = adminAccountsCollection.Find(filter).FirstOrDefault();

        //     if (adminAccounts == null)
        //     {
        //         return NotFound();
        //     }

        //     return View(adminAccounts);
        // }


        public ActionResult Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("AdminAccounts ID is required.");
            }

            var database = _client.GetDatabase("DemoMongoDb");
            var adminAccountsCollection = database.GetCollection<AdminAccounts>("AdminAccounts");
            var rolesCollection = database.GetCollection<Roles>("Roles");
            var roles = rolesCollection.Find(_ => true).ToList();

            ViewBag.Roles = roles;

            var adminAccounts = adminAccountsCollection.Find(n => n._id == id).FirstOrDefault();

            if (adminAccounts == null)
            {
                return NotFound();
            }

            return View(adminAccounts);
        }

        // POST: Admin/AdminadminAccounts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(string id, AdminAccounts adminAccounts, string idRoles)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("adminAccounts ID is required.");
            }

            if (!ModelState.IsValid)
            {
                return View(adminAccounts);
            }

            var database = _client.GetDatabase("DemoMongoDb");
            var adminAccountsCollection = database.GetCollection<AdminAccounts>("AdminAccounts");

            var filter = Builders<AdminAccounts>.Filter.Eq("_id", id);

            string salt = Utilities.GetRandomKey();
            adminAccounts.Salt = salt;
            adminAccounts.Password = (adminAccounts.Password + salt.Trim()).ToMD5();

            var rolesCollection = database.GetCollection<Roles>("Roles");
            var roleName = rolesCollection.Find(r => r._id == idRoles).FirstOrDefault();
            if (roleName != null)
            {
                adminAccounts.Role = roleName.RoleName;
            }

            var update = Builders<AdminAccounts>.Update
                .Set("FullName", adminAccounts.FullName)
                .Set("Address", adminAccounts.Address)
                .Set("Email", adminAccounts.Email)
                .Set("Phone", adminAccounts.Phone)
                .Set("Role", adminAccounts.Role)
                .Set("RoleId", adminAccounts.RoleId)
                .Set("Password", adminAccounts.Password)
                .Set("Active", adminAccounts.Active);

            var result = adminAccountsCollection.UpdateOne(filter, update);
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
                return BadRequest("AdminAccounts ID is required.");
            }

            if (!ObjectId.TryParse(id, out ObjectId objectId))
            {
                return BadRequest("Invalid AdminAccounts ID format.");
            }

            var database = _client.GetDatabase("DemoMongoDb");
            var adminAccountsCollection = database.GetCollection<AdminAccounts>("AdminAccounts");

            var filter = Builders<AdminAccounts>.Filter.Eq("_id", objectId);
            var adminAccounts = adminAccountsCollection.Find(filter).FirstOrDefault();

            if (adminAccounts == null)
            {
                return NotFound();
            }

            return View(adminAccounts);
        }

        // POST: Admin/AdminadminAccounts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("AdminAccounts ID is required.");
            }

            var database = _client.GetDatabase("DemoMongoDb");
            var adminAccountsCollection = database.GetCollection<AdminAccounts>("AdminAccounts");

            var result = adminAccountsCollection.DeleteOne(n => n._id == id);
            if (result.DeletedCount == 0)
            {
                return NotFound();
            }

            return RedirectToAction("Index");
        }
    }
}
