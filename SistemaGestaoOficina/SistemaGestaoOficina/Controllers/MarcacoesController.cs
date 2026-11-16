
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
        public async Task<IActionResult> Create()
        {
            var clientes = await _marcacaoRepository.GetClientesAsync();

            var veiculos = await _marcacaoRepository.GetVeiculosAsync();

            var mecanicos = await _marcacaoRepository.GetMecanicosAsync();

            var tiposReparacao = await _marcacaoRepository.GetTiposReparacaoAsync();


            ViewBag.Clientes = new SelectList(clientes, "Id", "FullName");

            ViewBag.Veiculos = new SelectList(
                veiculos.Select(v => new
                {
                    v.Id,
                    Descricao = $"{v.Marca} - {v.Modelo} - {v.Matricula}"
                }),
                "Id",
                "Descricao"
                );

            ViewBag.Mecanicos = new SelectList(mecanicos, "Id", "Nome");

            ViewBag.TiposServico = new SelectList(tiposReparacao, "Nome", "Nome");

            return View();
        
        }

        // POST: Marcacoes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Marcacao marcacao)
        {
            marcacao.Estado = "Agendada";

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
                else if (marcacao.DataHora.DayOfWeek == DayOfWeek.Sunday ||
                         marcacao.DataHora.Minute != 0 ||
                         marcacao.DataHora.Second != 0 ||
                         marcacao.DataHora.Hour < 9 ||
                         marcacao.DataHora.Hour >=
                            (marcacao.DataHora.DayOfWeek == DayOfWeek.Saturday ? 13 : 18))
                {
                    ModelState.AddModelError(
                        nameof(Marcacao.DataHora),
                        "Selecione um horário válido da oficina.");
                }
                else if (await _marcacaoRepository.ExisteConflitoAsync(marcacao))
                {
                    ModelState.AddModelError(
                        nameof(Marcacao.DataHora),
                        "O mecânico já tem uma marcação neste horário.");
                }
                else if (await _marcacaoRepository.ExisteMarcacaoDuplicadaAsync(marcacao))
                {
                    ModelState.AddModelError(
                        "",
                        "Este cliente já possui uma marcação agendada para este veículo e serviço. Cancele a marcação anterior antes de criar outra.");
                }
                else
                {
                    await _marcacaoRepository.CreateAsync(marcacao);

                    TempData["SuccessMessage"] =
                        "Marcação criada com sucesso!";

                    return RedirectToAction(nameof(Index));
                }
            }

            // Recarregar os dados do formulário
            var clientes = await _marcacaoRepository.GetClientesAsync();
            var veiculos = await _marcacaoRepository.GetVeiculosAsync();
            var mecanicos = await _marcacaoRepository.GetMecanicosAsync();
            var tiposReparacao = await _marcacaoRepository.GetTiposReparacaoAsync();

            ViewBag.Clientes = new SelectList(
                clientes, "Id", "FullName", marcacao.UserId);

            ViewBag.Veiculos = new SelectList(
                veiculos.Select(v => new
                {
                    v.Id,
                    Descricao = $"{v.Marca} - {v.Modelo} - {v.Matricula}"
                }),
                "Id",
                "Descricao",
                marcacao.VeiculoId);

            ViewBag.Mecanicos = new SelectList(
                mecanicos, "Id", "Nome", marcacao.MecanicoId);

            ViewBag.TiposServico = new SelectList(
                tiposReparacao, "Nome", "Nome", marcacao.TipoServico);

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

            if (marcacao.Estado != "Agendada")
            {
                TempData["ErrorMessage"] = "Apenas marcações agendadas podem ser editadas.";

                return RedirectToAction(nameof(Index));
            }

            var clientes = await _marcacaoRepository.GetClientesAsync();
            var veiculos = await _marcacaoRepository.GetVeiculosAsync();
            var mecanicos = await _marcacaoRepository.GetMecanicosAsync();
            var tiposReparacao = await _marcacaoRepository.GetTiposReparacaoAsync();

            ViewBag.Clientes = new SelectList(clientes, "Id", "FullName", marcacao.UserId);

            ViewBag.Veiculos = new SelectList(veiculos.Select(v => new
                {
                    v.Id,
                    Descricao = $"{v.Marca} - {v.Modelo} - {v.Matricula}"
                }),

                "Id", "Descricao", marcacao.VeiculoId);

            ViewBag.Mecanicos = new SelectList(mecanicos, "Id", "Nome", marcacao.MecanicoId);

            ViewBag.TiposServico = new SelectList(tiposReparacao, "Nome", "Nome", marcacao.TipoServico);

            return View(marcacao);
        }


        // POST: Marcacoes/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Marcacao marcacao)
        {
            var marcacaoExistente =
               await _marcacaoRepository.GetByIdAsync(marcacao.Id);

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

            // Manter o cliente e o estado originais
            marcacao.UserId = marcacaoExistente.UserId;
            marcacao.Estado = marcacaoExistente.Estado;

            if (ModelState.IsValid)
            {
                if (!await _marcacaoRepository.DadosValidosAsync(marcacao))
                {
                    ModelState.AddModelError("", "Cliente, veículo ou mecânico inválido.");
                }
                else if (marcacao.DataHora <= DateTime.Now)
                {
                    ModelState.AddModelError(nameof(Marcacao.DataHora), "A data deve ser futura.");
                }
                else if (marcacao.DataHora.DayOfWeek == DayOfWeek.Sunday ||
                         marcacao.DataHora.Minute != 0 ||
                         marcacao.DataHora.Second != 0 ||
                         marcacao.DataHora.Hour < 9 ||
                         marcacao.DataHora.Hour >=
                            (marcacao.DataHora.DayOfWeek == DayOfWeek.Saturday ? 13 : 18))
                {
                    ModelState.AddModelError(nameof(Marcacao.DataHora), "Selecione um horário válido da oficina.");
                }
                else if (await _marcacaoRepository.ExisteConflitoAsync(marcacao))
                {
                    ModelState.AddModelError(nameof(Marcacao.DataHora), "O mecânico já tem uma marcação neste horário.");
                }
                else if (await _marcacaoRepository.ExisteMarcacaoDuplicadaAsync(marcacao))
                {
                    ModelState.AddModelError(
                        "",
                        "Este cliente já possui uma marcação agendada para este veículo e serviço. Cancele a marcação anterior antes de criar outra.");
                }
                else
                {
                    await _marcacaoRepository.UpdateAsync(marcacao);

                    TempData["SuccessMessage"] = "Marcação atualizada com sucesso!";

                    return RedirectToAction(nameof(Index));
                }
            }

            // Recarregar os dados do formulário
            var clientes = await _marcacaoRepository.GetClientesAsync();
            var veiculos = await _marcacaoRepository.GetVeiculosAsync();
            var mecanicos = await _marcacaoRepository.GetMecanicosAsync();
            var tiposReparacao = await _marcacaoRepository.GetTiposReparacaoAsync();

            ViewBag.Clientes = new SelectList(clientes, "Id", "FullName", marcacao.UserId);

            // Apenas veículos pertencentes ao cliente
            ViewBag.Veiculos = new SelectList(veiculos
                    .Where(v => v.UserId == marcacao.UserId)
                    .Select(v => new
                    {
                        v.Id,
                        Descricao = $"{v.Marca} - {v.Modelo} - {v.Matricula}"
                    }),
                "Id",
                "Descricao",
                marcacao.VeiculoId);

            ViewBag.Mecanicos = new SelectList(mecanicos, "Id", "Nome", marcacao.MecanicoId);

            ViewBag.TiposServico = new SelectList(tiposReparacao, "Nome", "Nome", marcacao.TipoServico);

            return View(marcacao);
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

                TempData["SuccessMessage"] = "Marcação apagada com sucesso!";
            }
            catch (DbUpdateException)
            {
                TempData["ErrorMessage"] = "Não é possível apagar esta marcação porque existem registos associados.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
