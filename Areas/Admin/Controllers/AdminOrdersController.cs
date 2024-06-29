using DemoMongoDB.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using PagedList.Core;

namespace DemoMongoDB.Controllers
{
    [Authorize(Roles = "Admin, Staff", Policy = "AdminAndStaffPolicy", AuthenticationSchemes = "AdminAuth, StaffAuth")]
    [Area("Admin")]
    public class AdminOrdersController : Controller
    {
        private readonly IMongoClient _client;

        public AdminOrdersController(IMongoClient client)
        {
            _client = client;
        }

        public ActionResult Index(int? page)
        {
            var database = _client.GetDatabase("DemoMongoDb");
            var ordersCollection = database.GetCollection<Orders>("Orders");
            var ordersQuery = ordersCollection.AsQueryable();

            var pageNumber = page == null || page <= 0 ? 1 : page.Value;
            var pageSize = 20;
            var pagedOrders = ordersQuery.ToPagedList(pageNumber, pageSize);

            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = pagedOrders.PageCount;

            return View(pagedOrders);
        }

        public ActionResult ChangeStatus(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("News ID is required.");
            }

            var database = _client.GetDatabase("DemoMongoDb");
            var ordersCollection = database.GetCollection<Orders>("Orders");

            var orders = ordersCollection.Find(n => n._id == id).FirstOrDefault();

            if (orders == null)
            {
                return NotFound();
            }

            return View(orders);
        }

        [HttpPost]
        public async Task<IActionResult> ChangeStatus(string id, string status)
        {
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(status))
            {
                return BadRequest("Order ID and status are required.");
            }

            if (!ObjectId.TryParse(id, out ObjectId objectId))
            {
                return BadRequest("Invalid Order ID format.");
            }

            var database = _client.GetDatabase("DemoMongoDb");
            var ordersCollection = database.GetCollection<Orders>("Orders");

            var filter = Builders<Orders>.Filter.Eq("_id", id);
            var update = Builders<Orders>.Update.Set(o => o.Status, status);

            var result = await ordersCollection.UpdateOneAsync(filter, update);

            if (result.MatchedCount == 0)
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
            var ordersCollection = database.GetCollection<Orders>("Orders");

            var filter = Builders<Orders>.Filter.Eq("_id", objectId);
            var orders = ordersCollection.Find(filter).FirstOrDefault();

            if (orders == null)
            {
                return NotFound();
            }

            return View(orders);
        }

        // POST: Admin/AdminNews/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("Orders ID is required.");
            }

            var database = _client.GetDatabase("DemoMongoDb");
            var ordersCollection = database.GetCollection<Orders>("Orders");

            var result = ordersCollection.DeleteOne(n => n._id == id);
            if (result.DeletedCount == 0)
            {
                return NotFound();
            }

            return RedirectToAction("Index");
        }
    }
}