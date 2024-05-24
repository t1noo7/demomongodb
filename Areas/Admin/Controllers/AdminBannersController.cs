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
    public class AdminBannersController : Controller
    {
        private readonly IMongoClient _client;

        public AdminBannersController(IMongoClient client)
        {
            _client = client;
        }

        public ActionResult Index(int? page)
        {

            var database = _client.GetDatabase("DemoMongoDb");
            var BannersCollection = database.GetCollection<Banners>("Banners");
            var BannersQuery = BannersCollection.AsQueryable();
            List<Banners> BannersDetails = BannersCollection.Find(_ => true).ToList();
            var pageNumber = page == null || page <= 0 ? 1 : page.Value;
            var pageSize = 20;
            //PagedList<Banners> models = new PagedList<Banners>(BannersDetails, pageNumber, pageSize);
            var pagedBanners = BannersQuery.ToPagedList(pageNumber, pageSize);
            ViewBag.CurrentPage = pageNumber;
            return View(pagedBanners);
        }
        // public ActionResult Index(int? page)
        // {
        //     var database = _client.GetDatabase("DemoMongoDb");
        //     var BannersCollection = database.GetCollection<Banners>("Banners");
        //     var BannersQuery = BannersCollection.AsQueryable();
        //     var pageSize = 20;
        //     var pageNumber = page ?? 1;
        //     var skipAmount = (pageNumber - 1) * pageSize;

        //     // Fetch a page of Banners items
        //     var pagedBanners = BannersQuery.Skip(skipAmount).Take(pageSize).ToList();

        //     ViewBag.CurrentPage = pageNumber;
        //     ViewBag.TotalPages = (int)Math.Ceiling(BannersQuery.Count() / (double)pageSize);

        //     return View(pagedBanners);
        // }


        public ActionResult Create()
        {
            return View();
        }

        // POST: Admin/AdminBanners/Create/5
        [HttpPost]
        public async Task<ActionResult> Create(Banners Banners, Microsoft.AspNetCore.Http.IFormFile fThumb)
        {
            var database = _client.GetDatabase("DemoMongoDb");
            var BannersCollection = database.GetCollection<Banners>("Banners");
            if (fThumb != null)
            {
                string extension = Path.GetExtension(fThumb.FileName);
                string image = Utilities.SEOUrl(Banners.BannerName) + extension;
                Banners.Thumb = await Utilities.UploadFile(fThumb, @"banners", image.ToLower());
            }
            if (string.IsNullOrEmpty(Banners.Thumb)) Banners.Thumb = "default.jpg";
            // Find the document with the highest OrderIndex
            var maxOrderIndexDocument = await BannersCollection.Find(Builders<Banners>.Filter.Empty)
                                                            .SortByDescending(b => b.OrderIndex)
                                                            .Limit(1)
                                                            .FirstOrDefaultAsync();
            int maxOrderIndex = maxOrderIndexDocument != null ? maxOrderIndexDocument.OrderIndex : 0;

            // Increment the maxOrderIndex by 1 for the new Banner
            Banners.OrderIndex = maxOrderIndex + 1;
            Banners.DateModified = DateTime.Now;
            Banners._id = null;
            BannersCollection.InsertOne(Banners);
            return RedirectToAction("Index");
        }



        public ActionResult Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("Banners ID is required.");
            }

            if (!ObjectId.TryParse(id, out ObjectId objectId))
            {
                return BadRequest("Invalid Banners ID format.");
            }

            var database = _client.GetDatabase("DemoMongoDb");
            var BannersCollection = database.GetCollection<Banners>("Banners");

            var filter = Builders<Banners>.Filter.Eq("_id", objectId);
            var Banners = BannersCollection.Find(filter).FirstOrDefault();

            if (Banners == null)
            {
                return NotFound();
            }

            return View(Banners);
        }


        public ActionResult Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("Banners ID is required.");
            }

            var database = _client.GetDatabase("DemoMongoDb");
            var BannersCollection = database.GetCollection<Banners>("Banners");

            var Banners = BannersCollection.Find(n => n._id == id).FirstOrDefault();

            if (Banners == null)
            {
                return NotFound();
            }
            ViewBag.OldOrderIndex = Banners.OrderIndex;
            return View(Banners);
        }

        // POST: Admin/AdminBanners/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(string id, Banners Banners, Microsoft.AspNetCore.Http.IFormFile fThumb)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("Banners ID is required.");
            }

            // if (!ModelState.IsValid)
            // {
            //     return View(Banners);
            // }

            var database = _client.GetDatabase("DemoMongoDb");
            var BannersCollection = database.GetCollection<Banners>("Banners");

            var filter = Builders<Banners>.Filter.Eq("_id", id);
            if (fThumb != null)
            {
                string extension = Path.GetExtension(fThumb.FileName);
                string image = Utilities.SEOUrl(Banners.BannerName) + extension;
                Banners.Thumb = await Utilities.UploadFile(fThumb, @"banners", image.ToLower());
            }

            int newOrderIndex = Banners.OrderIndex;

            // Kiểm tra xem có bản ghi khác có OrderIndex trùng với OrderIndex mới không
            var filterOtherBanner = Builders<Banners>.Filter.Where(b => b._id != id && b.OrderIndex == newOrderIndex);
            var otherBanner = await BannersCollection.Find(filterOtherBanner).FirstOrDefaultAsync();

            if (otherBanner != null)
            {
                // Nếu có, swap giá trị OrderIndex của hai bản ghi
                int oldOrderIndex = Convert.ToInt32(Request.Form["oldOrderIndex"]);
                otherBanner.OrderIndex = Banners.OrderIndex;


                // Cập nhật bản ghi vào cơ sở dữ liệu
                var updateOtherBanner = Builders<Banners>.Update.Set("OrderIndex", oldOrderIndex);
                var updateResult = await BannersCollection.UpdateOneAsync(filterOtherBanner, updateOtherBanner);
            }
            if (string.IsNullOrEmpty(Banners.Thumb)) Banners.Thumb = "default.jpg";
            var update = Builders<Banners>.Update
                .Set("BannerName", Banners.BannerName)
                .Set("BannerHeaderText", Banners.BannerHeaderText)
                .Set("BannerText", Banners.BannerText)
                .Set("DateModified", DateTime.Now)
                .Set("ActiveButton", Banners.ActiveButton)
                .Set("ButtonText", Banners.ButtonText)
                .Set("Active", Banners.Active)
                .Set("OrderIndex", Banners.OrderIndex)
                .Set("Thumb", Banners.Thumb);

            var result = BannersCollection.UpdateOne(filter, update);
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
                return BadRequest("Banners ID is required.");
            }

            if (!ObjectId.TryParse(id, out ObjectId objectId))
            {
                return BadRequest("Invalid Banners ID format.");
            }

            var database = _client.GetDatabase("DemoMongoDb");
            var BannersCollection = database.GetCollection<Banners>("Banners");

            var filter = Builders<Banners>.Filter.Eq("_id", objectId);
            var Banners = BannersCollection.Find(filter).FirstOrDefault();

            if (Banners == null)
            {
                return NotFound();
            }

            return View(Banners);
        }

        // POST: Admin/AdminBanners/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("Banners ID is required.");
            }

            var database = _client.GetDatabase("DemoMongoDb");
            var BannersCollection = database.GetCollection<Banners>("Banners");

            var result = BannersCollection.DeleteOne(n => n._id == id);
            if (result.DeletedCount == 0)
            {
                return NotFound();
            }

            return RedirectToAction("Index");
        }
    }
}
