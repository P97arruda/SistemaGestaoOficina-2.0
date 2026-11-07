using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaGestaoOficina.Data.Entities;
using SistemaGestaoOficina.Data.Repository;

namespace SistemaGestaoOficina.Controllers
{
    [Authorize(Roles = "Admin")]
    public class TiposReparacoesController : Controller
    {

        private readonly ITipoReparacaoRepository _repository;

        public TiposReparacoesController(ITipoReparacaoRepository repository)
        {
            _repository = repository;
        }

        //Listra
        public IActionResult Index()
        {
            return View(_repository.GetAll());
        }

        // CRIAR
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TipoReparacao model)
        {
            if (!ModelState.IsValid)
                return View(model);

            await _repository.CreateAsync(model);

            TempData["SuccessMessage"] = "Tipo de reparação criado com sucesso.";
            return RedirectToAction(nameof(Index));
        }

        // EDITAR
        public async Task<IActionResult> Edit(int id)
        {
            var tipo = await _repository.GetByIdAsync(id);

            if (tipo == null)
                return NotFound();

            return View(tipo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TipoReparacao model)
        {
            if (id != model.Id)
                return NotFound();

            if (!ModelState.IsValid)
                return View(model);

            var tipo = await _repository.GetByIdAsync(id);

            if (tipo == null)
                return NotFound();

            tipo.Nome = model.Nome;

            await _repository.UpdateAsync(tipo);

            TempData["SuccessMessage"] = "Tipo de reparação atualizado com sucesso.";
            return RedirectToAction(nameof(Index));
        }


        // APAGAR
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var tipo = await _repository.GetByIdAsync(id);

            if (tipo == null)
                return NotFound();

            try
            {
                await _repository.DeleteAsync(tipo);
                TempData["SuccessMessage"] = "Tipo de reparação apagado com sucesso.";
            }
            catch (DbUpdateException)
            {
                TempData["ErrorMessage"] =
                    "Não é possível apagar este tipo de reparação porque está associado a uma reparação.";
            }

            return RedirectToAction(nameof(Index));
        }

    }
}
