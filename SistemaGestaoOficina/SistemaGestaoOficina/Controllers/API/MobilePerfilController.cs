
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
        public async Task<IActionResult> GetMeuPerfil()
        {
            var cliente = await _userManager.GetUserAsync(User);

            if (cliente == null)
            {
                return Unauthorized();
            }

            var perfil = new MobilePerfilDto
            {
                FirstName = cliente.FirstName,
                LastName = cliente.LastName,
                Email = cliente.Email ?? string.Empty,
                PhoneNumber = cliente.PhoneNumber ?? string.Empty
            };

            return Ok(perfil);
        }
    }
}
