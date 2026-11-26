
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SistemaGestaoOficina.Data.Entities;
using SistemaGestaoOficina.DTO;

namespace SistemaGestaoOficina.Controllers.API.Mobile
{
    [ApiController]
    [Route("api/mobile/cliente")]
    [Authorize(
        AuthenticationSchemes = "MobileBearer",
        Roles = "Cliente")]
    public class MobilePerfilController : ControllerBase
    {
        private readonly UserManager<User> _userManager;

        public MobilePerfilController(
            UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        // GET: api/mobile/cliente/perfil
        [HttpGet("perfil")]
        public async Task<IActionResult> GetPerfil()
        {
            // Recuperar o cliente autenticado
            var cliente = await _userManager.GetUserAsync(User);

            if (cliente == null)
            {
                return Unauthorized();
            }

            // Preparar os dados do perfil
            var perfil = new MobilePerfilDto
            {
                FirstName = cliente.FirstName,
                LastName = cliente.LastName,
                NIF = cliente.NIF ?? string.Empty,
                Email = cliente.Email ?? string.Empty,
                PhoneNumber = cliente.PhoneNumber ?? string.Empty,

                // Fotografia do perfil
                FotoUrl = cliente.ImageId != Guid.Empty
                    ? cliente.ImageFullPath
                    : null
            };

            return Ok(perfil);
        }
    }
}
