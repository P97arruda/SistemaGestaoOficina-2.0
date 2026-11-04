using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaGestaoOficina.Helpers;
using SistemaGestaoOficina.Models;

namespace SistemaGestaoOficina.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserHelper _userHelper;
        private readonly IBlobHelper _blobHelper;
        private readonly IMailHelper _mailHelper;

        public AccountController(IUserHelper userHelper, IBlobHelper blobHelper, IMailHelper mailHelpe)
        {
            _userHelper = userHelper;
            _blobHelper = blobHelper;
            _mailHelper = mailHelpe;
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

                        if (await _userHelper.IsUserInRoleAsync(user, "Rececionista"))
                        {
                            return RedirectToAction("Index", "RececionistaArea");
                        }
                    }

                    return RedirectToAction("Index", "Home");
                }
            }

            ModelState.AddModelError(string.Empty, "Email ou password incorretos.");

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
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user =
                await _userHelper.GetUserByEmailAsync(model.UserName);

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

                        await _userHelper.ConfirmEmailAsync(user, confirmationToken);
                    }

                    TempData["SuccessMessage"] = "Password definida com sucesso. Já pode entrar na sua conta.";

                    return RedirectToAction("Login", "Account");
                }

                ViewBag.Message = "Erro ao definir a password.";

                return View(model);
            }

            ViewBag.Message = "Utilizador não encontrado.";

            return View(model);
        }


        [Authorize]
        [HttpGet]
        public async Task<IActionResult> EditarPerfil()
        {
            var email = User.Identity?.Name;

            if (string.IsNullOrWhiteSpace(email))
            {
                return Challenge();
            }

            var user = await _userHelper.GetUserByEmailAsync(email);

            if (user == null)
            {
                return NotFound();
            }

            var model = new EditarPerfilViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email!,
                PhoneNumber = user.PhoneNumber,
                NIF = user.NIF,
                ImageFullPath = user.ImageFullPath
            };

            return View(model);
        }
    

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarPerfil(EditarPerfilViewModel model)
        {
            var email = User.Identity?.Name;

            if (string.IsNullOrWhiteSpace(email))
            {
                return Challenge();
            }

            var user = await _userHelper.GetUserByEmailAsync(email);

            if (user == null)
            {
                return NotFound();
            }

            // A fotografia atual não é enviada pelo formulário.
            ModelState.Remove(nameof(model.ImageFullPath));

            if (model.ImageFile != null)
            {
                var extension = Path.GetExtension(
                    model.ImageFile.FileName).ToLowerInvariant();

                if (extension != ".jpg" &&
                    extension != ".jpeg" &&
                    extension != ".png" &&
                    extension != ".webp")
                {
                    ModelState.AddModelError(
                        nameof(model.ImageFile),
                        "A fotografia deve ser JPG, PNG ou WebP.");
                }

                if (model.ImageFile.Length == 0 ||
                    model.ImageFile.Length > 2 * 1024 * 1024)
                {
                    ModelState.AddModelError(
                        nameof(model.ImageFile),
                        "A fotografia deve ter no máximo 2 MB.");
                }
            }

            if (!ModelState.IsValid)
            {
                model.ImageFullPath = user.ImageFullPath;
                return View(model);
            }

            // Verificar se o email já pertence a outra conta
            var existingEmail =
                await _userHelper.GetUserByEmailAsync(model.Email);

            if (existingEmail != null && existingEmail.Id != user.Id)
            {
                ModelState.AddModelError(
                    nameof(model.Email),
                    "Este email já está registado.");

                model.ImageFullPath = user.ImageFullPath;
                return View(model);
            }

            // Verificar se o contacto já pertence a outra conta
            if (!string.IsNullOrWhiteSpace(model.PhoneNumber))
            {
                var existingPhone =
                    await _userHelper.GetUserByPhoneNumberAsync(model.PhoneNumber);

                if (existingPhone != null && existingPhone.Id != user.Id)
                {
                    ModelState.AddModelError(
                        nameof(model.PhoneNumber),
                        "Este contacto já está registado.");

                    model.ImageFullPath = user.ImageFullPath;
                    return View(model);
                }
            }

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.PhoneNumber = model.PhoneNumber;

            // Nesta primeira etapa, mantemos o email atual.
            // A alteração de email terá confirmação própria.
            if (model.Email != user.Email)
            {
                ModelState.AddModelError(
                    nameof(model.Email),
                    "A alteração de email será disponibilizada posteriormente.");

                model.ImageFullPath = user.ImageFullPath;
                return View(model);
            }

            // Guardar nova fotografia
            if (model.ImageFile != null)
            {
                try
                {
                    user.ImageId = await _blobHelper.UploadBlobAsync(
                        model.ImageFile,
                        "profile");
                }
                catch
                {
                    ModelState.AddModelError(
                        nameof(model.ImageFile),
                        "Não foi possível guardar a fotografia.");

                    model.ImageFullPath = user.ImageFullPath;
                    return View(model);
                }
            }

            var result = await _userHelper.UpdateUserAsync(user);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        error.Description);
                }

                model.ImageFullPath = user.ImageFullPath;
                return View(model);
            }

            TempData["SuccessMessage"] = "Perfil atualizado com sucesso.";

            if (await _userHelper.IsUserInRoleAsync(user, "Admin"))
            {
                return RedirectToAction("Index", "Admin");
            }

            if (await _userHelper.IsUserInRoleAsync(user, "Rececionista"))
            {
                return RedirectToAction("Index", "RececionistaArea");
            }

            if (await _userHelper.IsUserInRoleAsync(user, "Cliente"))
            {
                return RedirectToAction("Index", "ClienteArea");
            }

            return RedirectToAction("Index", "Home");

        }

        // GET: Account/ChangePassword
        [Authorize]
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View(new ChangePasswordViewModel());
        }



        // POST: Account/ChangePassword
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Buscar o utilizador autenticado
            var email = User.Identity?.Name;

            if (string.IsNullOrWhiteSpace(email))
            {
                return Challenge();
            }

            var user = await _userHelper.GetUserByEmailAsync(email);

            if (user == null)
            {
                return NotFound();
            }

            // Alterar a password através do Identity
            var result = await _userHelper.ChangePasswordAsync(
                user,
                model.OldPassword,
                model.NewPassword);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return View(model);
            }

            // Enviar email de confirmação
            if (!string.IsNullOrWhiteSpace(user.Email))
            {
                var subject = "Password alterada - OficinaCET107";

                var body = $@"
                    <h2>OficinaCET107</h2>
                    <p>Olá, {System.Net.WebUtility.HtmlEncode(user.FirstName)}!</p>
                    <p>A password da sua conta foi alterada com sucesso.</p>
                    <p>Se foi você, não precisa de fazer mais nada.</p>
                    <p>Se não reconhece esta alteração, contacte o suporte.</p>
                    <p>Equipa OficinaCET107</p>";

                try
                {
                    _mailHelper.SendEmail(user.Email, subject, body);
                }
                catch (Exception)
                {
                    // A password já foi alterada.
                    // Uma falha no email não impede a operação.
                }
            }

            TempData["SuccessMessage"] = "Password alterada com sucesso.";

            // Redirecionar para o painel correspondente
            if (await _userHelper.IsUserInRoleAsync(user, "Admin"))
            {
                return RedirectToAction("Index", "Admin");
            }

            if (await _userHelper.IsUserInRoleAsync(user, "Rececionista"))
            {
                return RedirectToAction("Index", "RececionistaArea");
            }

            if (await _userHelper.IsUserInRoleAsync(user, "Cliente"))
            {
                return RedirectToAction("Index", "ClienteArea");
            }

            return RedirectToAction("Index", "Home");
        }



        // GET: Account/RecoverPassword
        [AllowAnonymous]
        [HttpGet]
        public IActionResult RecoverPassword()
        {
            return View(new RecoverPasswordViewModel());
        }

        // POST: Account/RecoverPassword
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RecoverPassword(
            RecoverPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userHelper.GetUserByEmailAsync(model.Email);

            if (user != null)
            {
                var token = await _userHelper
                    .GeneratePasswordResetTokenAsync(user);

                var link = Url.Action(
                    "ResetPassword",
                    "Account",
                    new
                    {
                        userName = user.Email,
                        token = token
                    },
                    protocol: Request.Scheme);

                var body = $@"
                    <h2>Recuperação de Password</h2>
                    <p>Olá!</p>
                    <p>Recebemos um pedido para recuperar a sua password.</p>
                    <p>Clique no link abaixo para definir uma nova:</p>
                    <p><a href=""{System.Net.WebUtility.HtmlEncode(link)}"">
                    Recuperar Password</a></p>
                    <p>Se não fez este pedido, ignore este email.</p>
                    <p>Equipa OficinaCET107</p>";

                try
                {
                    _mailHelper.SendEmail(
                        user.Email,
                        "Recuperar Password - OficinaCET107",
                        body);
                }
                catch (Exception)
                {
                    // Não revelar informações sobre a conta.
                }
            }

            TempData["SuccessMessage"] =
                "Se o email estiver registado, receberá um link para recuperar a password.";

            return RedirectToAction(nameof(RecoverPassword));
        }


    }
}