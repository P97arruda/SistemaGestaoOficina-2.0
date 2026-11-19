
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaGestaoOficina.Data.Entities;
using SistemaGestaoOficina.Data.Repository;
using SistemaGestaoOficina.Models;

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
        public async Task<IActionResult> Index()
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
        public async Task<IActionResult> Create()
        {
            await CarregarListasAsync();

            return View(new MarcacaoViewModel());
        }

        // POST: Marcacoes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MarcacaoViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Mapeamento manual: ViewModel -> Entity
                var marcacao = new Marcacao
                {
                    UserId = model.UserId,
                    VeiculoId = model.VeiculoId!.Value,
                    MecanicoId = model.MecanicoId!.Value,
                    TipoServico = model.TipoServico,
                    DataHora = model.DataHora!.Value,
                    Estado = "Agendada"
                };

                await ValidarMarcacaoAsync(marcacao);

                if (ModelState.IsValid)
                {
                    await _marcacaoRepository.CreateAsync(marcacao);

                    TempData["SuccessMessage"] = "Marcação criada com sucesso!";

                    return RedirectToAction(nameof(Index));
                }
            }

            await CarregarListasAsync(model);

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
                TempData["ErrorMessage"] = "Apenas marcações agendadas podem ser editadas.";

                return RedirectToAction(nameof(Index));
            }

            // Mapeamento manual: Entity -> ViewModel
            var model = new MarcacaoViewModel
            {
                Id = marcacao.Id,
                UserId = marcacao.UserId,
                VeiculoId = marcacao.VeiculoId,
                MecanicoId = marcacao.MecanicoId,
                TipoServico = marcacao.TipoServico,
                DataHora = marcacao.DataHora
            };

            await CarregarListasAsync(model);

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
                TempData["ErrorMessage"] = "Apenas marcações agendadas podem ser editadas.";

                return RedirectToAction(nameof(Index));
            }

            // O cliente não pode ser alterado na edição.
            model.UserId = marcacaoExistente.UserId;
            ModelState.Remove(nameof(MarcacaoViewModel.UserId));

            if (ModelState.IsValid)
            {
                // Criar uma entidade temporária para validar
                // sem alterar o registo original.
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

                await ValidarMarcacaoAsync(dados);

                if (ModelState.IsValid)
                {
                    // Atualizar somente os campos permitidos.
                    marcacaoExistente.VeiculoId = dados.VeiculoId;
                    marcacaoExistente.MecanicoId = dados.MecanicoId;
                    marcacaoExistente.TipoServico = dados.TipoServico;
                    marcacaoExistente.DataHora = dados.DataHora;

                    await _marcacaoRepository.UpdateAsync(marcacaoExistente);

                    TempData["SuccessMessage"] = "Marcação atualizada com sucesso!";

                    return RedirectToAction(nameof(Index));
                }
            }

            await CarregarListasAsync(model);

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
                TempData["ErrorMessage"] = "Esta marcação não pode ser cancelada.";

                return RedirectToAction(nameof(Index));
            }

            marcacao.Estado = "Cancelada";

            await _marcacaoRepository.UpdateAsync(marcacao);

            TempData["SuccessMessage"] = "Marcação cancelada com sucesso!";

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
                TempData["ErrorMessage"] = "Apenas marcações agendadas podem ser apagadas.";

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
                TempData["ErrorMessage"] = "Não é possível apagar esta marcação porque existem registos associados.";
            }

            return RedirectToAction(nameof(Index));
        }

        // Validações de negócio
        private async Task ValidarMarcacaoAsync(Marcacao marcacao)
        {
            if (!await _marcacaoRepository.DadosValidosAsync(marcacao))
            {
                ModelState.AddModelError("", "Cliente, veículo ou mecânico inválido.");

                return;
            }

            if (marcacao.DataHora <= DateTime.Now)
            {
                ModelState.AddModelError(nameof(MarcacaoViewModel.DataHora), "A data deve ser futura.");

                return;
            }

            if (marcacao.DataHora.DayOfWeek == DayOfWeek.Sunday ||
                marcacao.DataHora.Minute != 0 ||
                marcacao.DataHora.Second != 0 ||
                marcacao.DataHora.Hour < 9 ||
                marcacao.DataHora.Hour >=
                    (marcacao.DataHora.DayOfWeek == DayOfWeek.Saturday ? 13 : 18))
            {
                ModelState.AddModelError(nameof(MarcacaoViewModel.DataHora), "Selecione um horário válido da oficina.");

                return;
            }

            if (await _marcacaoRepository.ExisteConflitoAsync(marcacao))
            {
                ModelState.AddModelError(nameof(MarcacaoViewModel.DataHora), "O mecânico já tem uma marcação neste horário.");

                return;
            }

            if (await _marcacaoRepository.ExisteMarcacaoDuplicadaAsync(marcacao))
            {
                ModelState.AddModelError("", "Este cliente já possui uma marcação agendada para este veículo e serviço. Cancele a marcação anterior antes de criar outra.");
            }
        }

        // Carregar opções dos formulários
        private async Task CarregarListasAsync(MarcacaoViewModel? model = null)
        {
            var clientes = await _marcacaoRepository.GetClientesAsync();
            var veiculos = await _marcacaoRepository.GetVeiculosAsync();
            var mecanicos = await _marcacaoRepository.GetMecanicosAsync();
            var tiposReparacao =
                await _marcacaoRepository.GetTiposReparacaoAsync();

            ViewBag.Clientes = new SelectList(clientes, "Id", "FullName", model?.UserId);

            var veiculosDisponiveis = model != null && !string.IsNullOrEmpty(model.UserId) ? veiculos.Where(v => v.UserId == model.UserId) : veiculos;

            ViewBag.Veiculos = new SelectList(veiculosDisponiveis.Select(v => new
                {
                    v.Id,
                    Descricao =
                        $"{v.Marca} - {v.Modelo} - {v.Matricula}"
                }),
                "Id",
                "Descricao",
                model?.VeiculoId);

            ViewBag.Mecanicos = new SelectList( mecanicos,"Id", "Nome", model?.MecanicoId);

            ViewBag.TiposServico = new SelectList(tiposReparacao, "Nome", "Nome", model?.TipoServico);
        }
    }
}
