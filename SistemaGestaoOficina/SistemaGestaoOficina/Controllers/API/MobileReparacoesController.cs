
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaGestaoOficina.Data.Entities;
using SistemaGestaoOficina.Data.Repository;
using SistemaGestaoOficina.DTO;

namespace SistemaGestaoOficina.Controllers.API.Mobile
{
    [ApiController]
    [Route("api/mobile/cliente")]
    [Authorize(
        AuthenticationSchemes = "MobileBearer",
        Roles = "Cliente")]
    public class MobileReparacoesController : ControllerBase
    {
        private readonly IReparacaoRepository _reparacaoRepository;
        private readonly UserManager<User> _userManager;

        public MobileReparacoesController(
            IReparacaoRepository reparacaoRepository,
            UserManager<User> userManager)
        {
            _reparacaoRepository = reparacaoRepository;
            _userManager = userManager;
        }

        // GET: api/mobile/cliente/reparacoes
        [HttpGet("reparacoes")]
        public async Task<IActionResult> GetHistoricoReparacoes()
        {
            var cliente = await _userManager.GetUserAsync(User);

            if (cliente == null)
            {
                return Unauthorized();
            }

            var reparacoes = await _reparacaoRepository
                .GetAll()
                .AsNoTracking()
                .Where(r =>
                    r.Concluida &&
                    !r.Cancelada &&
                    r.Marcacao != null &&
                    r.Marcacao.UserId == cliente.Id &&
                    r.Marcacao.Veiculo != null &&
                    r.Marcacao.Veiculo.UserId == cliente.Id)
                .OrderByDescending(r => r.DataFim)
                .Select(r => new MobileReparacaoDto
                {
                    Id = r.Id,
                    TipoServico = r.TipoServico,
                    DataInicio = r.DataInicio,
                    DataFim = r.DataFim,

                    Veiculo = r.Marcacao!.Veiculo!.Marca + " " +
                              r.Marcacao.Veiculo.Modelo,

                    Matricula = r.Marcacao.Veiculo.Matricula,

                    CustoMaoDeObra = r.CustoMaoDeObra,
                    CustoTotal = r.CustoTotal
                })
                .ToListAsync();

            return Ok(reparacoes);
        }

        // GET: api/mobile/cliente/reparacoes/5
        [HttpGet("reparacoes/{id:int}")]
        public async Task<IActionResult> GetDetalhesReparacao(int id)
        {
            var cliente = await _userManager.GetUserAsync(User);

            if (cliente == null)
            {
                return Unauthorized();
            }

            var reparacao = await _reparacaoRepository
                .GetAll()
                .AsNoTracking()
                .Where(r =>
                    r.Id == id &&
                    r.Concluida &&
                    !r.Cancelada &&
                    r.Marcacao != null &&
                    r.Marcacao.UserId == cliente.Id &&
                    r.Marcacao.Veiculo != null &&
                    r.Marcacao.Veiculo.UserId == cliente.Id)
                .Select(r => new MobileReparacaoDto
                {
                    Id = r.Id,
                    TipoServico = r.TipoServico,
                    DataInicio = r.DataInicio,
                    DataFim = r.DataFim,

                    Veiculo = r.Marcacao!.Veiculo!.Marca + " " +
                              r.Marcacao.Veiculo.Modelo,

                    Matricula = r.Marcacao.Veiculo.Matricula,

                    CustoMaoDeObra = r.CustoMaoDeObra,
                    CustoTotal = r.CustoTotal
                })
                .FirstOrDefaultAsync();

            if (reparacao == null)
            {
                return NotFound(new
                {
                    mensagem = "Reparação não encontrada."
                });
            }

            return Ok(reparacao);
        }
    }
}
