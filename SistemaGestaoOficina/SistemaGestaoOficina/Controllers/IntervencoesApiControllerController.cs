
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaGestaoOficina.Data.Repository;

namespace SistemaGestaoOficina.Controllers
{
    [ApiController]
    [Route("api/intervencoes")]
    [Authorize(Roles = "Admin,Rececionista,Mecanico")]
    public class IntervencoesApiController : ControllerBase
    {
        private readonly IReparacaoRepository _reparacaoRepository;

        public IntervencoesApiController(
            IReparacaoRepository reparacaoRepository)
        {
            _reparacaoRepository = reparacaoRepository;
        }

        // GET: api/intervencoes
        [HttpGet]
        public async Task<IActionResult> GetTodasIntervencoes()
        {
            var intervencoes = await _reparacaoRepository
                .GetAll()
                .AsNoTracking()
                .OrderByDescending(r => r.DataInicio)
                .Select(r => new
                {
                    r.Id,
                    r.TipoServico,
                    r.DataInicio,
                    r.DataFim,
                    r.Concluida,
                    r.Cancelada,
                    r.CustoMaoDeObra,
                    r.CustoTotal
                })
                .ToListAsync();

            return Ok(intervencoes);
        }

        // GET: api/intervencoes/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetIntervencaoPorId(int id)
        {
            var intervencao = await _reparacaoRepository
                .GetAll()
                .AsNoTracking()
                .Where(r => r.Id == id)
                .Select(r => new
                {
                    r.Id,
                    r.TipoServico,
                    r.DataInicio,
                    r.DataFim,
                    r.Concluida,
                    r.Cancelada,
                    r.CustoMaoDeObra,
                    r.CustoTotal,

                    Pecas = r.ReparacaoPecas.Select(p => new
                    {
                        Nome = p.Peca != null
                            ? p.Peca.Nome
                            : "Peça não identificada",

                        p.Quantidade,
                        p.PrecoUnitario,

                        Subtotal = p.Quantidade * p.PrecoUnitario
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (intervencao == null)
            {
                return NotFound(new
                {
                    mensagem = "Intervenção não encontrada."
                });
            }

            return Ok(intervencao);
        }
    }
}
