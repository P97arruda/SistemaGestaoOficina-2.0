
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SistemaGestaoOficina.Data.Entities;
using SistemaGestaoOficina.DTO;

namespace SistemaGestaoOficina.Controllers.API.Mobile
{
    [ApiController]
    [Route("api/mobile")]
    public class MobileAuthController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public MobileAuthController(
            UserManager<User> userManager,
            SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // POST: api/mobile/login
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(
            [FromBody] MobileLoginDto model)
        {
            var user = await _userManager
                .FindByEmailAsync(model.Email.Trim());

            if (user == null)
            {
                return Unauthorized(new
                {
                    mensagem = "Email ou password inválidos."
                });
            }

            // Validar password e condições do Identity
            var result = await _signInManager
                .CheckPasswordSignInAsync(
                    user,
                    model.Password,
                    lockoutOnFailure: true);

            if (!result.Succeeded)
            {
                return Unauthorized(new
                {
                    mensagem = "Email ou password inválidos ou login indisponível."
                });
            }

            // Apenas clientes podem entrar no aplicativo
            var isCliente = await _userManager
                .IsInRoleAsync(user, "Cliente");

            if (!isCliente)
            {
                return Unauthorized(new
                {
                    mensagem = "Acesso não autorizado."
                });
            }

            // O login mobile não suporta autenticação de dois fatores
            if (await _userManager.GetTwoFactorEnabledAsync(user))
            {
                return Unauthorized(new
                {
                    mensagem = "Utilize o site para iniciar sessão nesta conta."
                });
            }

            // Criar identidade do cliente
            var principal = await _signInManager
                .CreateUserPrincipalAsync(user);

            // Gerar token para o aplicativo mobile
            return SignIn(principal, "MobileBearer");
        }
    }
}
