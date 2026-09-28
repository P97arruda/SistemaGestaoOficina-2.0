using Microsoft.AspNetCore.Mvc;

namespace SistemaGestaoOficina.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
