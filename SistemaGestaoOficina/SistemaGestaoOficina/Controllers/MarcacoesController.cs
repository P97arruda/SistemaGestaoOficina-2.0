
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaGestaoOficina.Data.Entities;
using SistemaGestaoOficina.Data.Repository;

namespace SistemaGestaoOficina.Controllers
{
    [Authorize(Roles = "Rececionista")]
    public class MarcacoesController : Controller
    {
        private readonly IMarcacaoRepository _marcacaoRepository;

        public MarcacoesController(IMarcacaoRepository marcacaoRepository)
        {
            _marcacaoRepository = marcacaoRepository;
        }

        // GET: Marcacoes
        public async Task<ActionResult> Index()
        {
            var marcacoes = await _marcacaoRepository.GetAll()
                .Include(m => m.User)
                .Include(m => m.Veiculo)
                .Include(m => m.Mecanico)
                .ToListAsync();

            return View(marcacoes);
        }

        // GET: Marcacoes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var marcacao = await _marcacaoRepository.GetAll()
                .Include(m => m.User)
                .Include(m => m.Veiculo)
                .Include(m => m.Mecanico)
                .FirstOrDefaultAsync(m => m.Id == id.Value);

            if (marcacao == null)
            {
                return NotFound();
            }

            return View(marcacao);
        }

        // GET: Marcacoes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Marcacoes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Marcacao marcacao)
        {
            if (ModelState.IsValid)
            {
                if (!await _marcacaoRepository.DadosValidosAsync(marcacao))
                {
                    ModelState.AddModelError(
                        "",
                        "Cliente, veículo ou mecânico inválido.");
                }
                else if (marcacao.DataHora <= DateTime.Now)
                {
                    ModelState.AddModelError(
                        nameof(Marcacao.DataHora),
                        "A data deve ser futura.");
                }
                else if (await _marcacaoRepository.ExisteConflitoAsync(marcacao))
                {
                    ModelState.AddModelError(
                        nameof(Marcacao.DataHora),
                        "O mecânico já tem uma marcação neste horário.");
                }
                else
                {
                    await _marcacaoRepository.CreateAsync(marcacao);

                    return RedirectToAction(nameof(Index));
                }
            }

            return View(marcacao);
        }

        // GET: Marcacoes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var marcacao = await _marcacaoRepository.GetByIdAsync(id.Value);

            if (marcacao == null)
            {
                return NotFound();
            }

            return View(marcacao);
        }

        // POST: Marcacoes/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Marcacao marcacao)
        {
            if (!await _marcacaoRepository.ExistAsync(marcacao.Id))
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                if (!await _marcacaoRepository.DadosValidosAsync(marcacao))
                {
                    ModelState.AddModelError(
                        "",
                        "Cliente, veículo ou mecânico inválido.");
                }
                else if (await _marcacaoRepository.ExisteConflitoAsync(marcacao))
                {
                    ModelState.AddModelError(
                        nameof(Marcacao.DataHora),
                        "O mecânico já tem uma marcação neste horário.");
                }
                else
                {
                    try
                    {
                        await _marcacaoRepository.UpdateAsync(marcacao);

                        return RedirectToAction(nameof(Index));
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!await _marcacaoRepository.ExistAsync(marcacao.Id))
                        {
                            return NotFound();
                        }

                        throw;
                    }
                }
            }

            return View(marcacao);
        }

        // GET: Marcacoes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var marcacao = await _marcacaoRepository.GetByIdAsync(id.Value);

            if (marcacao == null)
            {
                return NotFound();
            }

            return View(marcacao);
        }

        // POST: Marcacoes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var marcacao = await _marcacaoRepository.GetByIdAsync(id);

            if (marcacao == null)
            {
                return NotFound();
            }

            try
            {
                await _marcacaoRepository.DeleteAsync(marcacao);

                TempData["SuccessMessage"] =
                    "Marcação eliminada com sucesso!";
            }
            catch (DbUpdateException)
            {
                TempData["ErrorMessage"] =
                    "Não é possível eliminar esta marcação.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
