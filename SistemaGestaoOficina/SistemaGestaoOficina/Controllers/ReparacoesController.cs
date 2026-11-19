using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaGestaoOficina.Data.Entities;
using SistemaGestaoOficina.Data.Repository;
using SistemaGestaoOficina.Models;

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
        // GET: Reparacoes/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            // 1. Carregar as marcações com os dados do cliente e veículo
            var marcacoes = await _marcacaoRepository
                .GetAll()
                .Include(m => m.User)
                .Include(m => m.Veiculo)
                .OrderByDescending(m => m.DataHora)
                .ToListAsync();

            // 2. Carregar os mecânicos ativos
            var mecanicos = await _mecanicoRepository
                .GetAll()
                .Where(m => m.Ativo)
                .OrderBy(m => m.Nome)
                .ToListAsync();

            // 3. Carregar as peças ativas
            var pecas = await _pecaRepository
                .GetAll()
                .Where(p => p.Ativa)
                .OrderBy(p => p.Nome)
                .ToListAsync();

            // 4. Enviar os dados para a View
            ViewBag.Marcacoes = marcacoes;
            ViewBag.Mecanicos = mecanicos;
            ViewBag.Pecas = pecas;

            // 5. Abrir o formulário com a data atual
            return View(new ReparacaoViewModel
            {
                DataInicio = DateTime.Now
            });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ReparacaoViewModel model, List<int> pecaIds, List<int> quantidades)
        {
            // Validar peças e quantidades
            if (pecaIds.Count != quantidades.Count)
            {
                ModelState.AddModelError("", "Os dados das peças são inválidos."
                );
            }

            // Validar mecânicos
            if (model.MecanicoIds == null || model.MecanicoIds.Count == 0)
            {
                ModelState.AddModelError("", "Selecione pelo menos um mecânico."
                );
            }

            // Validar marcação
            if (!model.MarcacaoId.HasValue)
            {
                ModelState.AddModelError(nameof(model.MarcacaoId), "Selecione uma marcação."
                );
            }
            else
            {
                var marcacao = await _marcacaoRepository.GetByIdAsync(model.MarcacaoId.Value);

                if (marcacao == null)
                {
                    ModelState.AddModelError(nameof(model.MarcacaoId), "A marcação selecionada não existe."
                    );
                }
            }

            // Validar datas
            if (model.DataFim.HasValue && model.DataInicio.HasValue && model.DataFim.Value < model.DataInicio.Value)
            {
                ModelState.AddModelError(nameof(model.DataFim), "A data de fim não pode ser anterior à data de início."
                );
            }

            // Validar os mecânicos selecionados
            var mecanicoIds = model.MecanicoIds?.Distinct().ToList() ?? new List<int>();

            foreach (var mecanicoId in mecanicoIds)
            {
                var mecanico = await _mecanicoRepository.GetByIdAsync(mecanicoId);

                if (mecanico == null || !mecanico.Ativo)
                {
                    ModelState.AddModelError("", "Um dos mecânicos selecionados é inválido."
                    );
                }
            }

            // Valor fixo por mecânico
            const decimal valorPorMecanico = 45.00m;

            // Calcular mão de obra
            decimal custoMaoDeObra = mecanicoIds.Count * valorPorMecanico;

            // Começar o total com a mão de obra
            decimal custoTotal = custoMaoDeObra;

            var reparacaoPecas = new List<ReparacaoPeca>();

            // Calcular o valor das peças
            if (pecaIds.Count == quantidades.Count)
            {
                for (int i = 0; i < pecaIds.Count; i++)
                {
                    var peca = await _pecaRepository.GetByIdAsync(pecaIds[i]);

                    if (peca == null || !peca.Ativa || quantidades[i] <= 0)
                    {
                        ModelState.AddModelError("", "Uma das peças selecionadas é inválida."
                        );

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

            // Guardar reparação
            if (ModelState.IsValid)
            {
                var reparacao = new Reparacao
                {
                    MarcacaoId = model.MarcacaoId!.Value,
                    TipoServico = model.TipoServico,
                    DataInicio = model.DataInicio!.Value,
                    DataFim = model.DataFim,
                    Concluida = model.Concluida,

                    // Custos
                    CustoMaoDeObra = custoMaoDeObra,
                    CustoTotal = custoTotal,

                    // Peças
                    ReparacaoPecas = reparacaoPecas,

                    // Mecânicos
                    MecanicoReparacaos = mecanicoIds.Select(id => new MecanicoReparacao
                        {
                            MecanicoId = id
                        })
                        .ToList()
                };

                await _reparacaoRepository.CreateAsync(reparacao);

                TempData["SuccessMessage"] ="Reparação criada com sucesso!";

                return RedirectToAction(nameof(Index));
            }

            // Recarregar as listas em caso de erro
            ViewBag.Marcacoes = await _marcacaoRepository.GetAll().OrderByDescending(m => m.DataHora).ToListAsync();

            ViewBag.Mecanicos = await _mecanicoRepository.GetAll().Where(m => m.Ativo).OrderBy(m => m.Nome).ToListAsync();

            ViewBag.Pecas = await _pecaRepository.GetAll().Where(p => p.Ativa).OrderBy(p => p.Nome).ToListAsync();

            return View(model);
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
