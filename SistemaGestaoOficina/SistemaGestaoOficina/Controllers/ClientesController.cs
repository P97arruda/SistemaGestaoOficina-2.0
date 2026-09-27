using Microsoft.AspNetCore.Mvc;
using SistemaGestaoOficina.Data;
using SistemaGestaoOficina.Data.Entities;

namespace SistemaGestaoOficina.Controllers
{
    public class ClientesController : Controller
    {
        private readonly IClienteRepository _clienteRepository;

        public ClientesController(IClienteRepository clienteRepository)
        {
            _clienteRepository = clienteRepository;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(_clienteRepository.GetAll());
        }


        
        public async Task<ActionResult> Details(int? id)
        {
            if(id == null)
            {
                return NotFound();
            }

            var cliente = await _clienteRepository.GetByIdAsync(id.Value);

            if(cliente == null)
            {
                return NotFound();
            }

            return View(cliente);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult>Create(Cliente cliente)
        {
            if (ModelState.IsValid)
            {
                await _clienteRepository.CreateAsync(cliente);
                return RedirectToAction(nameof(Index));
            }

            return View(cliente);
        }


        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cliente = await _clienteRepository.GetByIdAsync(id.Value);
            if (cliente == null)
            {
                return NotFound();
            }
            return View(cliente);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Cliente cliente)
        {
            if (ModelState.IsValid)
            {
                await _clienteRepository.UpdateAsync(cliente);
                return RedirectToAction(nameof(Index));
            }

            return View(cliente);
        }


        public async Task<IActionResult> Delete(int? id)
        {
            if(id == null)
            {
                return NotFound();
            }

            var cliente = await _clienteRepository.GetByIdAsync(id.Value);

            if(cliente == null)
            {
                return NotFound();
            }

            await _clienteRepository.DeleteAsync(cliente);

            return RedirectToAction(nameof(Index));
        }
    }
}
