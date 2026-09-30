using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SistemaGestaoOficina.Controllers
{
    [Authorize(Roles = "Cliente")]
    public class ClienteAreaController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}