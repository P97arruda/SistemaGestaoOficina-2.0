using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaGestaoOficina.Data.Entities;
using SistemaGestaoOficina.Data.Repository;

namespace SistemaGestaoOficina.Controllers
{
    [Authorize(Roles = "Rececionista")]
    public class ReparacoesController : Controller
    {
        private readonly IReparacaoRepository _reparacaoRepository;
        private readonly IMarcacaoRepository _marcacaoRepository;

        private readonly IMecanicoRepository _mecanicoRepository;
        private readonly IPecaRepository _pecaRepository;
        

        public ReparacoesController(
            IReparacaoRepository reparacaoRepository,
            IMarcacaoRepository marcacaoRepository,
            IPecaRepository pecaRepository, 
            IMecanicoRepository mecanicoRepository)
        {
            _reparacaoRepository = reparacaoRepository;
            _marcacaoRepository = marcacaoRepository;
            _pecaRepository = pecaRepository;
            _mecanicoRepository = mecanicoRepository;
        }

        public async Task<IActionResult> Index()
        {
            var reparacoes = await _reparacaoRepository.
                GetAll().
                ToListAsync();

            return View(reparacoes);
        }

        // GET: Reparacoes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reparacao = await _reparacaoRepository.GetByIdAsync(id.Value);

            if (reparacao == null)
            {
                return NotFound();
            }

            return View(reparacao);
        }

        // GET: Reparacoes/Create
        public async Task <IActionResult> Create()
        {
            // Carregar as marcações
            var marcacoes = await _marcacaoRepository
                .GetAll()
                .OrderByDescending(m => m.DataHora)
                .ToListAsync();

            // Carregar os mecânicos ativos
            var mecanicos = await _mecanicoRepository
                .GetAll()
                .Where(m => m.Ativo)
                .OrderBy(m => m.Nome)
                .ToListAsync();

            // Carregar as peças ativas
            var pecas = await _pecaRepository
                .GetAll()
                .Where(p => p.Ativa)
                .OrderBy(p => p.Nome)
                .ToListAsync();

            // Enviar as listas para a View
            ViewBag.Marcacoes = marcacoes;
            ViewBag.Mecanicos = mecanicos;
            ViewBag.Pecas = pecas;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Reparacao reparacao)
        {
            if (ModelState.IsValid)
            {
                await _reparacaoRepository.CreateAsync(reparacao);

                TempData["SuccessMessage"] = "Reparação criada com sucesso!";

                return RedirectToAction(nameof(Index));
            }

            return View(reparacao);
        }

        // GET: Reparacoes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reparacao = await _reparacaoRepository.GetByIdAsync(id.Value);

            if (reparacao == null)
            {
                return NotFound();
            }

            return View(reparacao);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Reparacao dados)
        {
            if (id != dados.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(dados);
            }

            var reparacao = await _reparacaoRepository.GetByIdAsync(id);

            if (reparacao == null)
            {
                return NotFound();
            }

            reparacao.TipoServico = dados.TipoServico;
            reparacao.DataInicio = dados.DataInicio;
            reparacao.DataFim = dados.DataFim;
            reparacao.CustoTotal = dados.CustoTotal;
            reparacao.Concluida = dados.Concluida;

            await _reparacaoRepository.UpdateAsync(reparacao);

            TempData["SuccessMessage"] = "Reparação atualizada com sucesso!";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var reparacao = await _reparacaoRepository.GetByIdAsync(id);

            if (reparacao == null)
            {
                return NotFound();
            }

            try
            {
                await _reparacaoRepository.DeleteAsync(reparacao);

                TempData["SuccessMessage"] = "Reparação eliminada com sucesso!";
            }
            catch (DbUpdateException)
            {
                TempData["ErrorMessage"] = "Não é possível eliminar esta reparação porque possui registos associados.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
