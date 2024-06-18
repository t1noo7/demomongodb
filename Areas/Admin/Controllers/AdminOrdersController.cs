using DemoMongoDB.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using PagedList.Core;

namespace DemoMongoDB.Controllers
{
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
    }
}