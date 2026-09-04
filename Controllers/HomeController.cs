using Microsoft.AspNetCore.Mvc;

namespace WebAPIDemoNew.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
