using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DemoMongoDB.Areas.Admin.Controllers
{
    
    public class HomeController : Controller
    {
        
        [Area("Admin")]
        [Route("/admin", Name ="admin")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
