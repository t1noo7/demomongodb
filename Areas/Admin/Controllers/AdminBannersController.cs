using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using DemoMongoDB.Models;
using PagedList.Core;
using DemoMongoDB.Helper;
using DemoMongoDB.ModelViews;
using Microsoft.AspNetCore.Authorization;

namespace DemoMongoDB.Controllers
{
    [Authorize(Roles = "Admin, Staff", Policy = "AdminAndStaffPolicy", AuthenticationSchemes = "AdminAuth, StaffAuth")]
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
                // Banners.Thumb = await Utilities.UploadFile(fThumb, @"banners", image.ToLower());

                Banners.Thumb = await Utilities.ResizeAndUploadImage(fThumb, "banners", desiredWidth: 942, desiredHeight: 600, image.ToLower());

            }
            if (string.IsNullOrEmpty(Banners.Thumb))
                Banners.Thumb = "default.jpg";

            var maxOrderIndexDocument = await BannersCollection.Find(Builders<Banners>.Filter.Empty)
                                                               .SortByDescending(b => b.OrderIndex)
                                                               .Limit(1)
                                                               .FirstOrDefaultAsync();
            int maxOrderIndex = maxOrderIndexDocument != null ? maxOrderIndexDocument.OrderIndex : 0;

            Banners.OrderIndex = maxOrderIndex + 1;
            Banners.DateModified = DateTime.Now;
            Banners._id = null;

            // Ensure position data is in "px" format
            Banners.BannerHeaderTextTop += "%";
            Banners.BannerHeaderTextLeft += "%";
            Banners.BannerTextTop += "%";
            Banners.BannerTextLeft += "%";
            Banners.BannerButtonTop += "%";
            Banners.BannerButtonLeft += "%";

            BannersCollection.InsertOne(Banners);

            return RedirectToAction("Index");
        }


        public ActionResult Preview()
        {
            HomeViewVM model = new HomeViewVM();

            var database = _client.GetDatabase("DemoMongoDb");
            var BannersCollection = database.GetCollection<Banners>("Banners");

            List<Banners> BannersDetails = BannersCollection.Find(x => x.Active == true).SortBy(x => x.OrderIndex).ToList();

            model.Banners = BannersDetails;

            return View(model);
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(string id, Banners Banners, Microsoft.AspNetCore.Http.IFormFile fThumb, string bannerHeaderTextPosition, string bannerTextPosition, string bannerButtonPosition)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("Banners ID is required.");
            }

            var database = _client.GetDatabase("DemoMongoDb");
            var BannersCollection = database.GetCollection<Banners>("Banners");

            var filter = Builders<Banners>.Filter.Eq("_id", id);
            if (fThumb != null)
            {
                string extension = Path.GetExtension(fThumb.FileName);
                string image = Utilities.SEOUrl(Banners.BannerName) + extension;
                // Banners.Thumb = await Utilities.UploadFile(fThumb, @"banners", image.ToLower());
                Banners.Thumb = await Utilities.ResizeAndUploadImage(fThumb, "banners", desiredWidth: 942, desiredHeight: 600, image.ToLower());
            }

            int newOrderIndex = Banners.OrderIndex;

            var filterOtherBanner = Builders<Banners>.Filter.Where(b => b._id != id && b.OrderIndex == newOrderIndex);
            var otherBanner = await BannersCollection.Find(filterOtherBanner).FirstOrDefaultAsync();

            if (otherBanner != null)
            {
                int oldOrderIndex = Convert.ToInt32(Request.Form["oldOrderIndex"]);
                otherBanner.OrderIndex = Banners.OrderIndex;

                var updateOtherBanner = Builders<Banners>.Update.Set("OrderIndex", oldOrderIndex);
                var updateResult = await BannersCollection.UpdateOneAsync(filterOtherBanner, updateOtherBanner);
            }
            if (string.IsNullOrEmpty(Banners.Thumb)) Banners.Thumb = "default.jpg";

            // Parse the position strings
            var headerTextPosition = bannerHeaderTextPosition.Split(',');
            var textPosition = bannerTextPosition.Split(',');
            var buttonPosition = bannerButtonPosition.Split(',');

            // Update the model with the positions
            Banners.BannerHeaderTextTop = headerTextPosition[0];
            Banners.BannerHeaderTextLeft = headerTextPosition[1];
            Banners.BannerTextTop = textPosition[0];
            Banners.BannerTextLeft = textPosition[1];
            Banners.BannerButtonTop = buttonPosition[0];
            Banners.BannerButtonLeft = buttonPosition[1];

            var update = Builders<Banners>.Update
                .Set("BannerName", Banners.BannerName)
                .Set("BannerHeaderText", Banners.BannerHeaderText)
                .Set("BannerText", Banners.BannerText)
                .Set("DateModified", DateTime.Now)
                .Set("ActiveButton", Banners.ActiveButton)
                .Set("ButtonText", Banners.ButtonText)
                .Set("Active", Banners.Active)
                .Set("OrderIndex", Banners.OrderIndex)
                .Set("Thumb", Banners.Thumb)
                .Set("BannerHeaderTextTop", Banners.BannerHeaderTextTop)
                .Set("BannerHeaderTextLeft", Banners.BannerHeaderTextLeft)
                .Set("BannerTextTop", Banners.BannerTextTop)
                .Set("BannerTextLeft", Banners.BannerTextLeft)
                .Set("BannerButtonTop", Banners.BannerButtonTop)
                .Set("BannerButtonLeft", Banners.BannerButtonLeft);

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
