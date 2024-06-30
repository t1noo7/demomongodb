using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DemoMongoDB.Areas.Admin.Controllers
{
    public class HomeController : Controller
    {
        [Authorize(Roles = "Admin", Policy = "AdminPolicy", AuthenticationSchemes = "AdminAuth")]
        [Area("Admin")]
        [Route("/admin", Name = "Admin")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
