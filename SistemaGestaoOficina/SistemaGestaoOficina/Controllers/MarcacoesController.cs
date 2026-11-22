
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaGestaoOficina.Data.Entities;
using SistemaGestaoOficina.Data.Repository;
using SistemaGestaoOficina.Models;

namespace SistemaGestaoOficina.Controllers
{
    [Authorize(Roles = "Rececionista, Mecanico")]
    public class MarcacoesController : Controller
    {
        private readonly IMarcacaoRepository _marcacaoRepository;

        public MarcacoesController(IMarcacaoRepository marcacaoRepository)
        {
            _marcacaoRepository = marcacaoRepository;
        }

        // GET: Marcacoes
        public async Task<IActionResult> Index()
        {
            var marcacoes = await _marcacaoRepository.GetAll()
                .Include(m => m.User)
                .Include(m => m.Veiculo)
                .Include(m => m.Mecanico)
                .ToListAsync();

            return View(marcacoes);
        }

        // GET: Marcacoes/GetVeiculosPorCliente
        [HttpGet]
        public async Task<IActionResult> GetVeiculosPorCliente(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Json(Array.Empty<object>());
            }

            var veiculos = await _marcacaoRepository
                .GetVeiculosPorClienteAsync(userId);

            return Json(veiculos.Select(v => new
            {
                id = v.Id,
                texto = $"{v.Marca} - {v.Modelo} - {v.Matricula}"
            }));
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
        public async Task<IActionResult> Create()
        {
            var clientes = await _marcacaoRepository.GetClientesAsync();
            var veiculos = await _marcacaoRepository.GetVeiculosAsync();
            var mecanicos = await _marcacaoRepository.GetMecanicosAsync();
            var tipos = await _marcacaoRepository.GetTiposReparacaoAsync();

            ViewBag.Clientes = new SelectList(
                clientes, "Id", "FullName");

            ViewBag.Veiculos = new SelectList(
                veiculos.Select(v => new
                {
                    v.Id,
                    Descricao = $"{v.Marca} - {v.Modelo} - {v.Matricula}"
                }),
                "Id", "Descricao");

            ViewBag.Mecanicos = new SelectList(
                mecanicos, "Id", "Nome");

            ViewBag.TiposServico = new SelectList(
                tipos, "Nome", "Nome");

            return View(new MarcacaoViewModel());
        }

        // POST: Marcacoes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MarcacaoViewModel model)
        {
            if (ModelState.IsValid)
            {
                var marcacao = new Marcacao
                {
                    UserId = model.UserId,
                    VeiculoId = model.VeiculoId!.Value,
                    MecanicoId = model.MecanicoId!.Value,
                    TipoServico = model.TipoServico,
                    DataHora = model.DataHora!.Value,
                    Estado = "Agendada"
                };

                var erros = await _marcacaoRepository
                    .ValidarMarcacaoAsync(marcacao);

                foreach (var erro in erros)
                {
                    ModelState.AddModelError("", erro);
                }

                if (ModelState.IsValid)
                {
                    await _marcacaoRepository.CreateAsync(marcacao);

                    TempData["SuccessMessage"] =
                        "Marcação criada com sucesso!";

                    return RedirectToAction(nameof(Index));
                }
            }

            var clientes = await _marcacaoRepository.GetClientesAsync();
            var veiculos = await _marcacaoRepository
                .GetVeiculosPorClienteAsync(model.UserId);
            var mecanicos = await _marcacaoRepository.GetMecanicosAsync();
            var tipos = await _marcacaoRepository.GetTiposReparacaoAsync();

            ViewBag.Clientes = new SelectList(
                clientes, "Id", "FullName", model.UserId);

            ViewBag.Veiculos = new SelectList(
                veiculos.Select(v => new
                {
                    v.Id,
                    Descricao = $"{v.Marca} - {v.Modelo} - {v.Matricula}"
                }),
                "Id", "Descricao", model.VeiculoId);

            ViewBag.Mecanicos = new SelectList(
                mecanicos, "Id", "Nome", model.MecanicoId);

            ViewBag.TiposServico = new SelectList(
                tipos, "Nome", "Nome", model.TipoServico);

            return View(model);
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

            if (marcacao.Estado != "Agendada")
            {
                TempData["ErrorMessage"] =
                    "Apenas marcações agendadas podem ser editadas.";

                return RedirectToAction(nameof(Index));
            }

            var model = new MarcacaoViewModel
            {
                Id = marcacao.Id,
                UserId = marcacao.UserId,
                VeiculoId = marcacao.VeiculoId,
                MecanicoId = marcacao.MecanicoId,
                TipoServico = marcacao.TipoServico,
                DataHora = marcacao.DataHora
            };

            var clientes = await _marcacaoRepository.GetClientesAsync();
            var veiculos = await _marcacaoRepository
                .GetVeiculosPorClienteAsync(model.UserId);
            var mecanicos = await _marcacaoRepository.GetMecanicosAsync();
            var tipos = await _marcacaoRepository.GetTiposReparacaoAsync();

            ViewBag.Clientes = new SelectList(
                clientes, "Id", "FullName", model.UserId);

            ViewBag.Veiculos = new SelectList(
                veiculos.Select(v => new
                {
                    v.Id,
                    Descricao = $"{v.Marca} - {v.Modelo} - {v.Matricula}"
                }),
                "Id", "Descricao", model.VeiculoId);

            ViewBag.Mecanicos = new SelectList(
                mecanicos, "Id", "Nome", model.MecanicoId);

            ViewBag.TiposServico = new SelectList(
                tipos, "Nome", "Nome", model.TipoServico);

            return View(model);
        }

        // POST: Marcacoes/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(MarcacaoViewModel model)
        {
            var marcacaoExistente =
                await _marcacaoRepository.GetByIdAsync(model.Id);

            if (marcacaoExistente == null)
            {
                return NotFound();
            }

            if (marcacaoExistente.Estado != "Agendada")
            {
                TempData["ErrorMessage"] =
                    "Apenas marcações agendadas podem ser editadas.";

                return RedirectToAction(nameof(Index));
            }

            model.UserId = marcacaoExistente.UserId;
            ModelState.Remove(nameof(MarcacaoViewModel.UserId));

            if (ModelState.IsValid)
            {
                var dados = new Marcacao
                {
                    Id = marcacaoExistente.Id,
                    UserId = marcacaoExistente.UserId,
                    VeiculoId = model.VeiculoId!.Value,
                    MecanicoId = model.MecanicoId!.Value,
                    TipoServico = model.TipoServico,
                    DataHora = model.DataHora!.Value,
                    Estado = marcacaoExistente.Estado
                };

                var erros = await _marcacaoRepository
                    .ValidarMarcacaoAsync(dados);

                foreach (var erro in erros)
                {
                    ModelState.AddModelError("", erro);
                }

                if (ModelState.IsValid)
                {
                    marcacaoExistente.VeiculoId = dados.VeiculoId;
                    marcacaoExistente.MecanicoId = dados.MecanicoId;
                    marcacaoExistente.TipoServico = dados.TipoServico;
                    marcacaoExistente.DataHora = dados.DataHora;

                    await _marcacaoRepository.UpdateAsync(marcacaoExistente);

                    TempData["SuccessMessage"] =
                        "Marcação atualizada com sucesso!";

                    return RedirectToAction(nameof(Index));
                }
            }

            var clientes = await _marcacaoRepository.GetClientesAsync();
            var veiculos = await _marcacaoRepository
                .GetVeiculosPorClienteAsync(model.UserId);
            var mecanicos = await _marcacaoRepository.GetMecanicosAsync();
            var tipos = await _marcacaoRepository.GetTiposReparacaoAsync();

            ViewBag.Clientes = new SelectList(
                clientes, "Id", "FullName", model.UserId);

            ViewBag.Veiculos = new SelectList(
                veiculos.Select(v => new
                {
                    v.Id,
                    Descricao = $"{v.Marca} - {v.Modelo} - {v.Matricula}"
                }),
                "Id", "Descricao", model.VeiculoId);

            ViewBag.Mecanicos = new SelectList(
                mecanicos, "Id", "Nome", model.MecanicoId);

            ViewBag.TiposServico = new SelectList(
                tipos, "Nome", "Nome", model.TipoServico);

            return View(model);
        }

        // POST: Marcacoes/Cancelar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancelar(int id)
        {
            var marcacao = await _marcacaoRepository.GetByIdAsync(id);

            if (marcacao == null)
            {
                return NotFound();
            }

            if (marcacao.Estado != "Agendada")
            {
                TempData["ErrorMessage"] =
                    "Esta marcação não pode ser cancelada.";

                return RedirectToAction(nameof(Index));
            }

            marcacao.Estado = "Cancelada";

            await _marcacaoRepository.UpdateAsync(marcacao);

            TempData["SuccessMessage"] =
                "Marcação cancelada com sucesso!";

            return RedirectToAction(nameof(Index));
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

            if (marcacao.Estado != "Agendada")
            {
                TempData["ErrorMessage"] =
                    "Apenas marcações agendadas podem ser apagadas.";

                return RedirectToAction(nameof(Index));
            }

            try
            {
                await _marcacaoRepository.DeleteAsync(marcacao);

                TempData["SuccessMessage"] =
                    "Marcação apagada com sucesso!";
            }
            catch (DbUpdateException)
            {
                TempData["ErrorMessage"] =
                    "Não é possível apagar esta marcação porque existem registos associados.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
