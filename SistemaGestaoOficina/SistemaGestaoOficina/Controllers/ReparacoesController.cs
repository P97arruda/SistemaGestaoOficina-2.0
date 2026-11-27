
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaGestaoOficina.Data.Entities;
using SistemaGestaoOficina.Data.Repository;
using SistemaGestaoOficina.Models;

namespace SistemaGestaoOficina.Controllers
{
    [Authorize(Roles = "Rececionista,Mecanico")]
    public class ReparacoesController : Controller
    {
        private readonly IReparacaoRepository _reparacaoRepository;
        private readonly IMarcacaoRepository _marcacaoRepository;
        private readonly IMecanicoRepository _mecanicoRepository;
        private readonly IPecaRepository _pecaRepository;

        private const decimal ValorPorMecanico = 45.00m;

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

        // GET: Reparacoes
        public async Task<IActionResult> Index()
        {
            var reparacoes = await _reparacaoRepository
                .GetAll()
                .Include(r => r.Marcacao)
                    .ThenInclude(m => m.User)
                .Include(r => r.Marcacao)
                    .ThenInclude(m => m.Veiculo)
                .Include(r => r.MecanicoReparacaos)
                    .ThenInclude(mr => mr.Mecanico)
                .OrderByDescending(r => r.DataInicio)
                .ToListAsync();

            return View(reparacoes);
        }

        // GET: Reparacoes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reparacao = await _reparacaoRepository
                .GetAll()
                .Include(r => r.Marcacao)
                    .ThenInclude(m => m.User)
                .Include(r => r.Marcacao)
                    .ThenInclude(m => m.Veiculo)
                .Include(r => r.MecanicoReparacaos)
                    .ThenInclude(mr => mr.Mecanico)
                .Include(r => r.ReparacaoPecas)
                    .ThenInclude(rp => rp.Peca)
                .FirstOrDefaultAsync(r => r.Id == id.Value);

            if (reparacao == null)
            {
                return NotFound();
            }

            return View(reparacao);
        }

        // GET: Reparacoes/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Marcacoes = await _marcacaoRepository
                .GetMarcacoesDisponiveisAsync();

            ViewBag.Mecanicos = await _mecanicoRepository
                .GetAll()
                .Where(m => m.Ativo)
                .OrderBy(m => m.Nome)
                .ToListAsync();

            ViewBag.Pecas = await _pecaRepository
                .GetAll()
                .Where(p => p.Ativa)
                .OrderBy(p => p.Nome)
                .ToListAsync();

            return View(new ReparacaoViewModel
            {
                DataInicio = DateTime.Now
            });
        }

        // POST: Reparacoes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            ReparacaoViewModel model,
            List<int> pecaIds,
            List<int> quantidades)
        {
            pecaIds ??= new List<int>();
            quantidades ??= new List<int>();

            // 1. VALIDAR A MARCAÇÃO
            Marcacao? marcacao = null;

            if (!model.MarcacaoId.HasValue)
            {
                ModelState.AddModelError(
                    nameof(model.MarcacaoId),
                    "Selecione uma marcação.");
            }
            else
            {
                marcacao = await _marcacaoRepository
                    .GetByIdAsync(model.MarcacaoId.Value);

                if (marcacao == null)
                {
                    ModelState.AddModelError(
                        nameof(model.MarcacaoId),
                        "A marcação selecionada não existe.");
                }
                else
                {
                    // Apenas marcações agendadas
                    if (marcacao.Estado != "Agendada")
                    {
                        ModelState.AddModelError(
                            nameof(model.MarcacaoId),
                            "Apenas marcações agendadas podem receber reparações.");
                    }

                    // Impedir alterar uma reparação
                    // concluída ou cancelada
                    bool reparacaoBloqueada = await _reparacaoRepository
                        .GetAll()
                        .AnyAsync(r =>
                            r.MarcacaoId == marcacao.Id &&
                            (r.Concluida || r.Cancelada));

                    if (reparacaoBloqueada)
                    {
                        ModelState.AddModelError(
                            nameof(model.MarcacaoId),
                            "Esta marcação já possui uma reparação concluída ou cancelada.");
                    }
                }
            }

            // 2. VALIDAR AS DATAS
            if (!model.DataInicio.HasValue)
            {
                ModelState.AddModelError(
                    nameof(model.DataInicio),
                    "Informe a data de início.");
            }

            if (!model.DataFim.HasValue)
            {
                ModelState.AddModelError(
                    nameof(model.DataFim),
                    "Informe a data de fim.");
            }

            if (model.DataInicio.HasValue &&
                model.DataFim.HasValue &&
                model.DataFim.Value < model.DataInicio.Value)
            {
                ModelState.AddModelError(
                    nameof(model.DataFim),
                    "A data de fim não pode ser anterior à data de início.");
            }

            // 3. VALIDAR OS MECÂNICOS
            var mecanicoIds = model.MecanicoIds?
                .Distinct()
                .ToList() ?? new List<int>();

            if (mecanicoIds.Count == 0)
            {
                ModelState.AddModelError(
                    "",
                    "Selecione pelo menos um mecânico.");
            }

            foreach (var mecanicoId in mecanicoIds)
            {
                var mecanico = await _mecanicoRepository
                    .GetByIdAsync(mecanicoId);

                if (mecanico == null || !mecanico.Ativo)
                {
                    ModelState.AddModelError(
                        "",
                        "Um dos mecânicos selecionados é inválido.");
                }
            }

            // 4. VALIDAR AS PEÇAS
            if (pecaIds.Count == 0)
            {
                ModelState.AddModelError(
                    "",
                    "Adicione pelo menos uma peça à reparação.");
            }

            if (pecaIds.Count != quantidades.Count)
            {
                ModelState.AddModelError(
                    "",
                    "Os dados das peças e quantidades são inválidos.");
            }

            if (pecaIds.Count != pecaIds.Distinct().Count())
            {
                ModelState.AddModelError(
                    "",
                    "A mesma peça não pode ser adicionada duas vezes.");
            }

            // 5. CALCULAR OS CUSTOS
            decimal custoMaoDeObra =
                mecanicoIds.Count * ValorPorMecanico;

            decimal custoTotal = custoMaoDeObra;

            var reparacaoPecas = new List<ReparacaoPeca>();

            if (pecaIds.Count == quantidades.Count)
            {
                for (int i = 0; i < pecaIds.Count; i++)
                {
                    var peca = await _pecaRepository
                        .GetByIdAsync(pecaIds[i]);

                    if (peca == null || !peca.Ativa)
                    {
                        ModelState.AddModelError(
                            "",
                            "Uma das peças selecionadas é inválida.");

                        continue;
                    }

                    if (quantidades[i] <= 0)
                    {
                        ModelState.AddModelError(
                            "",
                            "A quantidade deve ser maior que zero.");

                        continue;
                    }

                    reparacaoPecas.Add(new ReparacaoPeca
                    {
                        PecaId = peca.Id,
                        Quantidade = quantidades[i],
                        PrecoUnitario = peca.Preco
                    });

                    custoTotal += peca.Preco * quantidades[i];
                }
            }

            // 6. VERIFICAR ERROS
            if (!ModelState.IsValid)
            {
                ViewBag.Marcacoes = await _marcacaoRepository
                    .GetMarcacoesDisponiveisAsync();

                ViewBag.Mecanicos = await _mecanicoRepository
                    .GetAll()
                    .Where(m => m.Ativo)
                    .OrderBy(m => m.Nome)
                    .ToListAsync();

                ViewBag.Pecas = await _pecaRepository
                    .GetAll()
                    .Where(p => p.Ativa)
                    .OrderBy(p => p.Nome)
                    .ToListAsync();

                return View(model);
            }

            // 7. VERIFICAR SE A REPARAÇÃO JÁ EXISTE
            var reparacaoExistente = await _reparacaoRepository
                .GetAll()
                .FirstOrDefaultAsync(r =>
                    r.MarcacaoId == marcacao!.Id);

            if (reparacaoExistente != null)
            {
                // Já existe uma reparação não concluída:
                // atualizar e concluir o mesmo registo.
                bool concluida = await _reparacaoRepository
                    .ConcluirExistenteAsync(
                        marcacao!.Id,
                        model.DataInicio!.Value,
                        model.DataFim!.Value,
                        reparacaoPecas,
                        mecanicoIds,
                        ValorPorMecanico);

                if (!concluida)
                {
                    TempData["ErrorMessage"] =
                        "Não foi possível concluir a reparação existente.";

                    return RedirectToAction(nameof(Index));
                }

                TempData["SuccessMessage"] =
                    "Reparação concluída com sucesso!";

                return RedirectToAction(nameof(Index));
            }

            // 8. CRIAR UMA NOVA REPARAÇÃO
            var reparacao = new Reparacao
            {
                MarcacaoId = marcacao!.Id,
                TipoServico = marcacao.TipoServico,

                DataInicio = model.DataInicio!.Value,
                DataFim = model.DataFim,

                Concluida = true,

                CustoMaoDeObra = custoMaoDeObra,
                CustoTotal = custoTotal,

                ReparacaoPecas = reparacaoPecas,

                MecanicoReparacaos = mecanicoIds
                    .Select(id => new MecanicoReparacao
                    {
                        MecanicoId = id
                    })
                    .ToList()
            };

            // 9. CONCLUIR A MARCAÇÃO
            marcacao.Estado = "Concluida";

            // 10. GUARDAR OS DADOS
            await _reparacaoRepository.CreateAsync(reparacao);

            await _marcacaoRepository.UpdateAsync(marcacao);

            TempData["SuccessMessage"] =
                "Reparação concluída com sucesso!";

            return RedirectToAction(nameof(Index));
        }
    }
}
