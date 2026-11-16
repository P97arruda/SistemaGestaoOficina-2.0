using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaGestaoOficina.Data.Entities;
using SistemaGestaoOficina.Data.Repository;

namespace SistemaGestaoOficina.Controllers
{
    [Authorize(Roles = "Admin")]
    public class PecasController : Controller
    {
        private readonly IPecaRepository _pecaRepository;

        public PecasController(IPecaRepository pecaRepository)
        {
            _pecaRepository = pecaRepository;
        }

        public async Task<IActionResult> Index()
        {
            var pecas = await _pecaRepository.GetAll().ToListAsync();

            return View(pecas);
        }

        // GET: Pecas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var peca = await _pecaRepository.GetByIdAsync(id.Value);

            if (peca == null)
            {
                return NotFound();
            }

            return View(peca);
        }


        // GET: Pecas/Create
        public IActionResult Create()
        {
            return View();
        }


        // POST: Pecas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Peca peca)
        {
            if (ModelState.IsValid)
            {
                await _pecaRepository.CreateAsync(peca);

                TempData["SuccessMessage"] = "Peça criada com sucesso.";

                return RedirectToAction(nameof(Index));
            }

            return View(peca);
        }

        // GET: Pecas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var peca = await _pecaRepository.GetByIdAsync(id.Value);

            if (peca == null)
            {
                return NotFound();
            }

            return View(peca);
        }

        // POST: Pecas/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Peca peca)
        {
            if (id != peca.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                await _pecaRepository.UpdateAsync(peca);

                TempData["SuccessMessage"] = "Peça atualizada com sucesso.";

                return RedirectToAction(nameof(Index));
            }

            return View(peca);
        }

        // GET: Pecas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var peca = await _pecaRepository.GetByIdAsync(id.Value);

            if (peca == null)
            {
                return NotFound();
            }

            return View(peca);
        }

        // POST: Pecas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var peca = await _pecaRepository.GetByIdAsync(id);

            if (peca == null)
            {
                return NotFound();
            }

            await _pecaRepository.DeleteAsync(peca);

            TempData["SuccessMessage"] = "Peça eliminada com sucesso.";

            return RedirectToAction(nameof(Index));
        }

    }
}
