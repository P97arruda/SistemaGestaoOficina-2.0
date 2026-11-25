
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaGestaoOficina.Data;
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
    public class MobileVeiculosController : ControllerBase
    {
        private readonly IVeiculoRepository _veiculoRepository;
        private readonly UserManager<User> _userManager;

        public MobileVeiculosController(
            IVeiculoRepository veiculoRepository,
            UserManager<User> userManager)
        {
            _veiculoRepository = veiculoRepository;
            _userManager = userManager;
        }

        // GET: api/mobile/cliente/veiculos
        [HttpGet("veiculos")]
        public async Task<IActionResult> GetMeusVeiculos()
        {
            var cliente = await _userManager.GetUserAsync(User);

            if (cliente == null)
                return Unauthorized();

            var veiculos = await _veiculoRepository
                .GetAll()
                .AsNoTracking()
                .Where(v => v.UserId == cliente.Id)
                .OrderBy(v => v.Marca)
                .Select(v => new MobileVeiculoDto
                {
                    Id = v.Id,
                    Marca = v.Marca,
                    Modelo = v.Modelo,
                    Matricula = v.Matricula
                })
                .ToListAsync();

            return Ok(veiculos);
        }

        // GET: api/mobile/cliente/veiculos/5
        [HttpGet("veiculos/{id:int}")]
        public async Task<IActionResult> GetDetalhesVeiculo(int id)
        {
            var cliente = await _userManager.GetUserAsync(User);

            if (cliente == null)
                return Unauthorized();

            var veiculo = await _veiculoRepository
                .GetAll()
                .AsNoTracking()
                .Where(v =>
                    v.Id == id &&
                    v.UserId == cliente.Id)
                .Select(v => new MobileVeiculoDto
                {
                    Id = v.Id,
                    Marca = v.Marca,
                    Modelo = v.Modelo,
                    Matricula = v.Matricula
                })
                .FirstOrDefaultAsync();

            if (veiculo == null)
                return NotFound(new
                {
                    mensagem = "Veículo não encontrado."
                });

            return Ok(veiculo);
        }
    }
}

