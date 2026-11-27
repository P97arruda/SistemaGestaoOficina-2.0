using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaGestaoOficina.Data.Repository;

namespace SistemaGestaoOficina.Controllers
{
    [AllowAnonymous]
    public class HomeController : Controller
    {
        private readonly IMecanicoRepository _mecanicoRepository;

        public HomeController(IMecanicoRepository mecanicoRepository)
        {
            _mecanicoRepository = mecanicoRepository;
        }

        public async Task<IActionResult> Index()
        {
            var mecanicosAtivos = await _mecanicoRepository
                .GetAll()
                .Include(m => m.User)
                .Where(m => m.Ativo)
                .OrderBy(m => m.Nome)
                .Take(5)
                .ToListAsync();

            ViewBag.MecanicosAtivos = mecanicosAtivos;

            return View();
        }
    }
}