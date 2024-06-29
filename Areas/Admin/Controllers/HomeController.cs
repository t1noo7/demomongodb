using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DemoMongoDB.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin, Staff", Policy = "AdminAndStaffPolicy", AuthenticationSchemes = "AdminAuthen, StaffAuthen")]
    public class HomeController : Controller
    {
        [Authorize(Roles = "Admin, Staff", Policy = "AdminAndStaffPolicy", AuthenticationSchemes = "AdminAuth, StaffAuth")]
        [Area("Admin")]
        [Route("/admin", Name = "Admin")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
