using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SistemaGestaoOficina.Controllers
{
    [Authorize(Roles = "Funcionario")]
    public class FuncionarioAreaController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
