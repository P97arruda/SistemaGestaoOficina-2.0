using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaGestaoOficina.Data.Entities;
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
        
        //GET: Mecanicos
        public async Task<ActionResult> Index()
        {
            var mecanicos = await _mecanicoRepository.GetAll().ToListAsync();

            return View(mecanicos);
        }

        // GET: Mecanicos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mecanico = await _mecanicoRepository.GetByIdAsync(id.Value);

            if (mecanico == null)
            {
                return NotFound();
            }

            return View(mecanico);
        }


        // GET: Mecanicos/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Mecanicos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Mecanico mecanico)
        {
            if (ModelState.IsValid)
            {
                await _mecanicoRepository.CreateAsync(mecanico);

                return RedirectToAction(nameof(Index));
            }

            return View(mecanico);
        }

        // GET: Mecanicos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mecanico = await _mecanicoRepository.GetByIdAsync(id.Value);

            if (mecanico == null)
            {
                return NotFound();
            }

            return View(mecanico);
        }

        // POST: Mecanicos/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Mecanico mecanico)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _mecanicoRepository.UpdateAsync(mecanico);
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _mecanicoRepository.ExistAsync(mecanico.Id))
                    {
                        return NotFound();
                    }

                    throw;
                }
            }

            return View(mecanico);
        }

        // GET: Mecanicos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mecanico = await _mecanicoRepository.GetByIdAsync(id.Value);

            if (mecanico == null)
            {
                return NotFound();
            }

            return View(mecanico);

        }

        // POST: Mecanicos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var mecanico = await _mecanicoRepository.GetByIdAsync(id);

            if (mecanico == null)
            {
                return NotFound();
            }

            try
            {
                await _mecanicoRepository.DeleteAsync(mecanico);

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("",
                    "Não é possível eliminar este mecânico porque possui registos associados.");

                return View("Delete", mecanico);
            }
        }


    }
