
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
    public class MobileMarcacoesController : ControllerBase
    {
        private readonly IMarcacaoRepository _marcacaoRepository;
        private readonly UserManager<User> _userManager;

        public MobileMarcacoesController(
            IMarcacaoRepository marcacaoRepository,
            UserManager<User> userManager)
        {
            _marcacaoRepository = marcacaoRepository;
            _userManager = userManager;
        }

        // GET: api/mobile/cliente/marcacoes
        [HttpGet("marcacoes")]
        public async Task<IActionResult> GetMinhasMarcacoes()
        {
            var cliente = await _userManager.GetUserAsync(User);

            if (cliente == null)
            {
                return Unauthorized();
            }

            var marcacoes = await _marcacaoRepository
                .GetAll()
                .AsNoTracking()
                .Where(m =>
                    m.UserId == cliente.Id &&
                    m.Veiculo != null &&
                    m.Veiculo.UserId == cliente.Id)
                .OrderByDescending(m => m.DataHora)
                .Select(m => new MobileMarcacaoDto
                {
                    Id = m.Id,
                    TipoServico = m.TipoServico,
                    DataHora = m.DataHora,
                    Estado = m.Estado,

                    Veiculo = m.Veiculo != null
                        ? m.Veiculo.Marca + " " + m.Veiculo.Modelo
                        : "Não informado",

                    Matricula = m.Veiculo != null
                        ? m.Veiculo.Matricula
                        : "",

                    Mecanico = m.Mecanico != null
                        ? m.Mecanico.Nome
                        : "A definir"
                })
                .ToListAsync();

            return Ok(marcacoes);
        }
    }
}
