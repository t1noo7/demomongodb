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
    [Authorize(Roles = "Admin", Policy = "AdminPolicy", AuthenticationSchemes = "AdminAuth")]
    [Area("Admin")]
    public class AdminPermissionsController : Controller
    {
        private readonly IMongoClient _client;

        public AdminPermissionsController(IMongoClient client)
        {
            _client = client;
        }

        public ActionResult Index(int? page)
        {
            var database = _client.GetDatabase("DemoMongoDb");
            var permissionsCollection = database.GetCollection<Permissions>("Permissions");
            var permissionsQuery = permissionsCollection.AsQueryable();
            List<Permissions> permissionsDetails = permissionsCollection.Find(_ => true).ToList();

            var pageNumber = page == null || page <= 0 ? 1 : page.Value;
            var pageSize = 20;
            var pagedPermissions = permissionsQuery.ToPagedList(pageNumber, pageSize);
            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = pagedPermissions.PageCount;

            var rolesCollection = database.GetCollection<Roles>("Roles");
            var rolesList = rolesCollection.Find(_ => true).ToList();
            var rolesDict = rolesList.ToDictionary(role => role._id.ToString(), role => role.RoleName);
            ViewBag.RolesDict = rolesDict;

            return View(pagedPermissions);
        }


        public ActionResult Create()
        {
            var database = _client.GetDatabase("DemoMongoDb");
            var functionsCollection = database.GetCollection<Functions>("Functions");
            var functionsList = functionsCollection.Find(_ => true).ToList();
            ViewBag.FunctionsList = functionsList;

            var rolesCollection = database.GetCollection<Roles>("Roles");
            var rolesList = rolesCollection.Find(_ => true).ToList();
            ViewBag.Roles = rolesList;

            return View(new Permissions());
        }

        // POST: Admin/AdminPermissions/Create
        [HttpPost]
        public ActionResult Create(Permissions model, List<string> functionIds, List<bool> accessPermissions, List<bool> canCreates, List<bool> canEdits, List<bool> canDeletes, List<bool> canReads)
        {
            var database = _client.GetDatabase("DemoMongoDb");
            var permissionsCollection = database.GetCollection<Permissions>("Permissions");

            // Check if RoleId already exists in the database
            var existingPermission = permissionsCollection.Find(p => p.RoleId == model.RoleId).FirstOrDefault();
            if (existingPermission != null)
            {
                TempData["ErrorMessage"] = "This role already has permissions assigned.";
                return RedirectToAction("Index");
            }


            // if (!ModelState.IsValid)
            // {
            //     // Capture and log model state errors
            //     var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            //     TempData["ErrorMessage"] = "Model state is invalid: " + string.Join(", ", errors);
            //     return RedirectToAction("Index");
            // }

            // Prepare the tasks list
            var tasksList = new List<Tasks>();
            for (int i = 0; i < functionIds.Count; i++)
            {
                var task = new Tasks
                {
                    FunctionId = functionIds[i],
                    AccessPermission = accessPermissions.Count > i && accessPermissions[i],
                    CanCreate = canCreates.Count > i && canCreates[i],
                    CanEdit = canEdits.Count > i && canEdits[i],
                    CanDelete = canDeletes.Count > i && canDeletes[i],
                    CanRead = canReads.Count > i && canReads[i]
                };
                tasksList.Add(task);
            }

            model.FunctionPermissions = tasksList;

            // Insert the new permission into the database
            permissionsCollection.InsertOne(model);

            return RedirectToAction("Index");
        }


        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var database = _client.GetDatabase("DemoMongoDb");
            var permissionsCollection = database.GetCollection<Permissions>("Permissions");
            var rolesCollection = database.GetCollection<Roles>("Roles");
            var functionsCollection = database.GetCollection<Functions>("Functions");

            var permission = await permissionsCollection.Find(p => p._id == id).FirstOrDefaultAsync();
            if (permission == null)
            {
                return NotFound();
            }

            var rolesList = await rolesCollection.Find(_ => true).ToListAsync();
            var functionsList = await functionsCollection.Find(_ => true).ToListAsync();

            ViewBag.Roles = rolesList;
            ViewBag.Functions = functionsList;

            return View(permission);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Permissions model)
        {
            var database = _client.GetDatabase("DemoMongoDb");
            var permissionsCollection = database.GetCollection<Permissions>("Permissions");

            if (ModelState.IsValid)
            {
                var existingPermission = await permissionsCollection.Find(p => p._id == model._id).FirstOrDefaultAsync();
                if (existingPermission != null)
                {
                    existingPermission.FunctionPermissions = model.FunctionPermissions;
                    await permissionsCollection.ReplaceOneAsync(p => p._id == model._id, existingPermission);
                    return RedirectToAction(nameof(Index));
                }
                return NotFound();
            }

            var rolesCollection = database.GetCollection<Roles>("Roles");
            var functionsCollection = database.GetCollection<Functions>("Functions");
            var rolesList = await rolesCollection.Find(_ => true).ToListAsync();
            var functionsList = await functionsCollection.Find(_ => true).ToListAsync();

            ViewBag.Roles = rolesList;
            ViewBag.Functions = functionsList;

            return View(model);
        }


        // GET: Admin/AdminPermissions/Delete/5
        public ActionResult Delete(string id)
        {
            var database = _client.GetDatabase("DemoMongoDb");
            var permissionsCollection = database.GetCollection<Permissions>("Permissions");
            var rolesCollection = database.GetCollection<Roles>("Roles");
            var functionsCollection = database.GetCollection<Functions>("Functions");

            var permission = permissionsCollection.Find(p => p._id == id).FirstOrDefault();
            if (permission == null)
            {
                return NotFound();
            }

            var role = rolesCollection.Find(r => r._id == permission.RoleId).FirstOrDefault();
            ViewBag.RoleName = role?.RoleName ?? "Unknown";

            var functionNames = functionsCollection.Find(_ => true).ToList().ToDictionary(f => f._id, f => f.FunctionName);
            ViewBag.FunctionNames = functionNames;

            return View(permission);
        }

        // POST: Admin/AdminPermissions/Delete/5
        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(string id)
        {
            var database = _client.GetDatabase("DemoMongoDb");
            var permissionsCollection = database.GetCollection<Permissions>("Permissions");
            permissionsCollection.DeleteOne(p => p._id == id);

            return RedirectToAction(nameof(Index));
        }

    }
}
