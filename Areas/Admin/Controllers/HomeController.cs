using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DemoMongoDB.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin, Staff", Policy = "AdminAndStaffPolicy", AuthenticationSchemes = "AdminAuthen, StaffAuthen")]
    public class HomeController : Controller
    {

        [Area("Admin")]
        [Route("/admin", Name = "admin")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
