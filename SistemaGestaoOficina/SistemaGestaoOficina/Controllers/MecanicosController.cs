
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaGestaoOficina.Data.Repository;

namespace SistemaGestaoOficina.Controllers
{
    [Authorize(Roles = "Admin")]
    public class MecanicosController : Controller
    {
        private readonly IMecanicoRepository _mecanicoRepository;

        public MecanicosController(IMecanicoRepository mecanicoRepository)
        {
            _mecanicoRepository = mecanicoRepository;
        }

        // GET: Mecanicos
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var mecanicos = await _mecanicoRepository
                .GetAll()
                .Include(m => m.User)
                .ToListAsync();

            return View(mecanicos);
        }

        // GET: Mecanicos/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var mecanico = await _mecanicoRepository.GetAll()
                .Include(m => m.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (mecanico == null)
                return NotFound();

            return View(mecanico);
        }
    }
}
