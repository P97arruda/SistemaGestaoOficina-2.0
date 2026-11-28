
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaGestaoOficina.Data.Entities;
using SistemaGestaoOficina.Data.Repository;
using SistemaGestaoOficina.Models;
using SistemaGestaoOficina.Services;

namespace SistemaGestaoOficina.Controllers
{
    [Authorize(Roles = "Rececionista, Mecanico")]
    public class MarcacoesController : Controller
    {
        private readonly IMarcacaoRepository _marcacaoRepository;
        private readonly IReparacaoRepository _reparacaoRepository;

        private readonly NotificacoesMarcacaoService _notificacoesMarcacaoService;

        public MarcacoesController(IMarcacaoRepository marcacaoRepository, IReparacaoRepository reparacaoRepository, NotificacoesMarcacaoService notificacoesMarcacaoService)
        {
            _marcacaoRepository = marcacaoRepository;
            _reparacaoRepository = reparacaoRepository;
            _notificacoesMarcacaoService = notificacoesMarcacaoService;
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MarcacaoViewModel model)
        {
            if (ModelState.IsValid)
            {
                // 1. CRIAR A MARCAÇÃO
                var marcacao = new Marcacao
                {
                    UserId = model.UserId,
                    VeiculoId = model.VeiculoId!.Value,
                    MecanicoId = model.MecanicoId!.Value,
                    TipoServico = model.TipoServico,
                    DataHora = model.DataHora!.Value,
                    Estado = "Agendada"
                };

                // 2. VALIDAR A MARCAÇÃO
                var erros = await _marcacaoRepository
                    .ValidarMarcacaoAsync(marcacao);

                foreach (var erro in erros)
                {
                    ModelState.AddModelError("", erro);
                }

                // 3. GUARDAR APENAS A MARCAÇÃO
                if (ModelState.IsValid)
                {
                    await _marcacaoRepository.CreateAsync(marcacao);

                    TempData["SuccessMessage"] =
                        "Marcação criada com sucesso!";

                    return RedirectToAction(nameof(Index));
                }
            }

            // 4. RECARREGAR OS DADOS DO FORMULÁRIO
            var clientes = await _marcacaoRepository.GetClientesAsync();

            var veiculos = await _marcacaoRepository
                .GetVeiculosPorClienteAsync(model.UserId);

            var mecanicos = await _marcacaoRepository
                .GetMecanicosAsync();

            var tipos = await _marcacaoRepository
                .GetTiposReparacaoAsync();

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



        [HttpGet]
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

            // APENAS MARCAÇÕES AGENDADAS PODEM SER EDITADAS

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

                // VALIDAR REGRAS DE NEGÓCIO

                var erros = await _marcacaoRepository
                    .ValidarMarcacaoAsync(dados);

                foreach (var erro in erros)
                {
                    ModelState.AddModelError("", erro);
                }

                if (ModelState.IsValid)
                {
                    // VERIFICAR SE ALGUM DADO FOI ALTERADO

                    bool houveAlteracao =
                        marcacaoExistente.VeiculoId != dados.VeiculoId ||
                        marcacaoExistente.MecanicoId != dados.MecanicoId ||
                        marcacaoExistente.TipoServico != dados.TipoServico ||
                        marcacaoExistente.DataHora != dados.DataHora;

                    // ATUALIZAR MARCAÇÃO

                    marcacaoExistente.VeiculoId = dados.VeiculoId;
                    marcacaoExistente.MecanicoId = dados.MecanicoId;
                    marcacaoExistente.TipoServico = dados.TipoServico;
                    marcacaoExistente.DataHora = dados.DataHora;

                    await _marcacaoRepository.UpdateAsync(
                        marcacaoExistente);

                    // ENVIAR EMAIL SE HOUVE ALTERAÇÃO
                    // E FOI REALIZADA POR UM FUNCIONÁRIO

                    bool alteradoPorFuncionario =
                        User.IsInRole("Admin") ||
                        User.IsInRole("Rececionista") ||
                        User.IsInRole("Mecanico");

                    if (houveAlteracao && alteradoPorFuncionario)
                    {
                        try
                        {
                            var detalhes = await _marcacaoRepository
                                .GetAll()
                                .Include(m => m.User)
                                .Include(m => m.Veiculo)
                                .FirstOrDefaultAsync(
                                    m => m.Id == marcacaoExistente.Id);

                            if (detalhes?.User?.Email != null &&
                                detalhes.Veiculo != null)
                            {
                                bool emailEnviado =
                                    _notificacoesMarcacaoService
                                        .EnviarAlteracao(
                                            detalhes.User.Email,
                                            detalhes.DataHora,
                                            detalhes.Veiculo.Marca,
                                            detalhes.Veiculo.Modelo,
                                            detalhes.Veiculo.Matricula,
                                            detalhes.TipoServico
                                        );

                                if (!emailEnviado)
                                {
                                    TempData["WarningMessage"] =
                                        "A marcação foi atualizada, " +
                                        "mas não foi possível enviar " +
                                        "o email ao cliente.";
                                }
                            }
                            else
                            {
                                TempData["WarningMessage"] =
                                    "A marcação foi atualizada, mas " +
                                    "não foi possível obter o email " +
                                    "ou os dados do veículo do cliente.";
                            }
                        }
                        catch
                        {
                            // FALHAR O EMAIL NÃO DESFAZ A ALTERAÇÃO

                            TempData["WarningMessage"] =
                                "A marcação foi atualizada, " +
                                "mas ocorreu um erro ao " +
                                "enviar a notificação.";
                        }
                    }

                    TempData["SuccessMessage"] =
                        "Marcação atualizada com sucesso!";

                    return RedirectToAction(nameof(Index));
                }
            }

            // RECARREGAR LISTAS SE HOUVER ERROS

            var clientes = await _marcacaoRepository.GetClientesAsync();

            var veiculos = await _marcacaoRepository
                .GetVeiculosPorClienteAsync(model.UserId);

            var mecanicos = await _marcacaoRepository.GetMecanicosAsync();

            var tipos = await _marcacaoRepository
                .GetTiposReparacaoAsync();

            ViewBag.Clientes = new SelectList(
                clientes,
                "Id",
                "FullName",
                model.UserId);

            ViewBag.Veiculos = new SelectList(
                veiculos.Select(v => new
                {
                    v.Id,
                    Descricao =
                        $"{v.Marca} - {v.Modelo} - {v.Matricula}"
                }),
                "Id",
                "Descricao",
                model.VeiculoId);

            ViewBag.Mecanicos = new SelectList(
                mecanicos,
                "Id",
                "Nome",
                model.MecanicoId);

            ViewBag.TiposServico = new SelectList(
                tipos,
                "Nome",
                "Nome",
                model.TipoServico);

            return View(model);
        }




        // POST: Marcacoes/Cancelar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancelar(int id)
        {
            // PROCURAR A MARCAÇÃO

            var marcacao = await _marcacaoRepository.GetByIdAsync(id);

            if (marcacao == null)
            {
                return NotFound();
            }

            // VERIFICAR SE PODE SER CANCELADA

            if (marcacao.Estado != "Agendada")
            {
                TempData["ErrorMessage"] =
                    "Esta marcação não pode ser cancelada.";

                return RedirectToAction(nameof(Index));
            }

            // CARREGAR DADOS DO CLIENTE E DO VEÍCULO

            var detalhes = await _marcacaoRepository.GetAll()
                .Include(m => m.User)
                .Include(m => m.Veiculo)
                .FirstOrDefaultAsync(m => m.Id == id);

            // CANCELAR MARCAÇÃO

            marcacao.Estado = "Cancelada";

            await _marcacaoRepository.UpdateAsync(marcacao);

            // ENVIAR EMAIL APENAS QUANDO UM FUNCIONÁRIO CANCELA

            bool canceladoPorFuncionario =
                User.IsInRole("Admin") ||
                User.IsInRole("Rececionista") ||
                User.IsInRole("Mecanico");

            if (canceladoPorFuncionario &&
                detalhes?.User?.Email != null &&
                detalhes.Veiculo != null)
            {
                bool emailEnviado =
                    _notificacoesMarcacaoService.EnviarCancelamento(
                        detalhes.User.Email,
                        detalhes.DataHora,
                        detalhes.Veiculo.Marca,
                        detalhes.Veiculo.Modelo,
                        detalhes.Veiculo.Matricula
                    );

                if (!emailEnviado)
                {
                    TempData["WarningMessage"] =
                        "Marcação cancelada, mas não foi possível " +
                        "enviar o email ao cliente.";
                }
            }

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
