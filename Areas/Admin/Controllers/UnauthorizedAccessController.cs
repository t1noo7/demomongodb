using Microsoft.AspNetCore.Mvc;
using DemoMongoDB.Models;

namespace DiChoSaiGon.Controllers
{
    [Route("/UnauthorizedAccess", Name = "Unauthorized Access")]
    public class UnauthorizedAccessController : Controller
    {
        public IActionResult Index()
        {
            return Content("Access is denied.");
        }
    }
}