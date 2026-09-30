using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaGestaoOficina.Helpers;
using SistemaGestaoOficina.Models;

namespace SistemaGestaoOficina.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserHelper _userHelper;

        public AccountController(IUserHelper userHelper)
        {
            _userHelper = userHelper;
        }


        public IActionResult Login()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _userHelper.LoginAsync(model);

                if (result.Succeeded)
                {
                    if (Request.Query.Keys.Contains("ReturnUrl"))
                    {
                        return Redirect(Request.Query["ReturnUrl"].First());
                    }

                    var user =
                        await _userHelper.GetUserByEmailAsync(model.Username);

                    if (user != null)
                    {
                        if (await _userHelper.IsUserInRoleAsync(user, "Admin"))
                        {
                            return RedirectToAction("Index", "Admin");
                        }

                        if (await _userHelper.IsUserInRoleAsync(user, "Cliente"))
                        {
                            return RedirectToAction("Index", "ClienteArea");
                        }

                        if (await _userHelper.IsUserInRoleAsync(user, "Funcionario"))
                        {
                            return RedirectToAction("Index", "FuncionarioArea");
                        }
                    }

                    return RedirectToAction("Index", "Home");
                }
            }

            ModelState.AddModelError(
                string.Empty,
                "Email ou password incorretos.");

            return View(model);
        }


        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _userHelper.LogoutAsync();

            return RedirectToAction("Index", "Home");
        }


        public IActionResult ResetPassword(string userName, string token)
        {
            var model = new ResetPasswordViewModel
            {
                UserName = userName,
                Token = token
            };

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(
            ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user =
                await _userHelper.GetUserByEmailAsync(model.UserName);

            if (user != null)
            {
                var result =
                    await _userHelper.ResetPasswordAsync(
                        user,
                        model.Token,
                        model.Password);

                if (result.Succeeded)
                {
                    if (!user.EmailConfirmed)
                    {
                        var confirmationToken =
                            await _userHelper
                                .GenerateEmailConfirmationTokenAsync(user);

                        await _userHelper.ConfirmEmailAsync(
                            user,
                            confirmationToken);
                    }

                    TempData["SuccessMessage"] =
                        "Password definida com sucesso. Já pode entrar na sua conta.";

                    return RedirectToAction(
                        "Login",
                        "Account");
                }

                ViewBag.Message =
                    "Erro ao definir a password.";

                return View(model);
            }

            ViewBag.Message =
                "Utilizador não encontrado.";

            return View(model);
        }
    }
}