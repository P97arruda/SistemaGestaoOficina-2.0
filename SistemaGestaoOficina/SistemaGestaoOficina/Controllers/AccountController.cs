using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SistemaGestaoOficina.Data.Entities;
using SistemaGestaoOficina.Helpers;
using SistemaGestaoOficina.Models;

namespace SistemaGestaoOficina.Controllers
{
    public class AccountController : Controller
    {

        private readonly IUserHelper _userHelper;
        private readonly IMailHelper _mailHelper;

        public AccountController(IUserHelper userHelper, IMailHelper mailHelper)
        {
            _userHelper = userHelper;
            _mailHelper = mailHelper;
        }

        public IActionResult Login()
        {
            //if (User.Identity != null && User.Identity.IsAuthenticated)
            //{
            //    return RedirectToAction("Index", "Home");
            //}

            return View();
        }


        [HttpPost]
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

                    return RedirectToAction("Index", "Clientes");
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


        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterNewUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userHelper.GetUserByEmailAsync(model.Username);

                if (user == null)
                {
                    user = new User
                    {
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        Email = model.Username,
                        UserName = model.Username,
                        PhoneNumber = model.PhoneNumber
                    };

                    var result = await _userHelper.AddUserAsync(user, model.Password);

                    if (result.Succeeded)
                    {
                        await _userHelper.AddUserToRoleAsync(user, "Cliente");

                        var token = await _userHelper.GenerateEmailConfirmationTokenAsync(user);

                        var link = Url.Action(
                            "ConfirmEmail",
                            "Account",
                            new
                            {
                                userId = user.Id,
                                token = token
                            },
                            protocol: HttpContext.Request.Scheme);

                            var response = _mailHelper.SendEmail(
                                model.Username,
                                "Confirmação de Email - OficinaCET107",
                                $"<h2>Bem-vindo à OficinaCET107</h2>" +
                                $"<p>Para confirmar a sua conta, clique no link:</p>" +
                                $"<a href=\"{link}\">Confirmar Email</a>");

                        if (!response.IsSuccess)
                        {
                            ModelState.AddModelError(
                                string.Empty,
                                response.Message);

                            return View(model);
                        }

                        ViewBag.Message =
                            "A conta foi criada. Verifique o seu email para confirmar a conta.";

                        return View(model);
                    }

                    ModelState.AddModelError(
                        string.Empty,
                        result.Errors.FirstOrDefault()?.Description
                        ?? "Erro ao criar o utilizador.");
                }
                else
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "Já existe um utilizador com este email.");
                }
            }

            return View(model);
        }

        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            {
                return NotFound();
            }

            var user = await _userHelper.GetUserByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            var result = await _userHelper.ConfirmEmailAsync(user, token);

            if (!result.Succeeded)
            {
                return NotFound();
            }

            return View();
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
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            var user = await _userHelper.GetUserByEmailAsync(model.UserName);

            if (user != null)
            {
                var result = await _userHelper.ResetPasswordAsync(
                    user,
                    model.Token,
                    model.Password);

                if (result.Succeeded)
                {
                    if (!user.EmailConfirmed)
                    {
                        var confirmationToken =
                            await _userHelper.GenerateEmailConfirmationTokenAsync(user);

                        await _userHelper.ConfirmEmailAsync(
                            user,
                            confirmationToken);
                    }

                    TempData["SuccessMessage"] =
                        "Password definida com sucesso. Já pode entrar na sua conta.";

                    return RedirectToAction("Login", "Account");
                }

                ViewBag.Message = "Erro ao definir a password.";
                return View(model);
            }

            ViewBag.Message = "Utilizador não encontrado.";
            return View(model);
        }

    }
}
