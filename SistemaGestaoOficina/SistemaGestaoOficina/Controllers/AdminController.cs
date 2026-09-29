using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaGestaoOficina.Data.Entities;
using SistemaGestaoOficina.Helpers;
using SistemaGestaoOficina.Models;

namespace SistemaGestaoOficina.Controllers
{
    
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IUserHelper _userHelper;
        private readonly IMailHelper _mailHelper;

        public AdminController(IUserHelper userHelper, IMailHelper mailHelper)
        {
            _userHelper = userHelper;
            _mailHelper = mailHelper;
        }



        public IActionResult Index()
        {
            return View();
        }


        public IActionResult CreateUser()
        {
            var model = new CreateUserViewModel();

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(CreateUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userByEmail = await _userHelper.GetUserByEmailAsync(model.Email);

                if (userByEmail != null)
                {
                    ModelState.AddModelError(
                        nameof(model.Email),
                        "Já existe um utilizador com este email.");

                    return View(model);
                }

                var userByPhone =
                    await _userHelper.GetUserByPhoneNumberAsync(model.PhoneNumber);

                if (userByPhone != null)
                {
                    ModelState.AddModelError(
                        nameof(model.PhoneNumber),
                        "Já existe um utilizador com este contacto.");

                    return View(model);
                }

                var user = new User
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    UserName = model.Email,
                    PhoneNumber = model.PhoneNumber
                };

                var result = await _userHelper.AddUserAsync(user, "Temp123!");

                if (result.Succeeded)
                {
                    await _userHelper.AddUserToRoleAsync(user, model.Role);

                    var token =
                        await _userHelper.GeneratePasswordResetTokenAsync(user);

                    var link = Url.Action(
                        "ResetPassword",
                        "Account",
                        new
                        {
                            userName = user.Email,
                            token = token
                        },
                        protocol: HttpContext.Request.Scheme);

                    var response = _mailHelper.SendEmail(
                        user.Email!,
                        "Criação de Conta - OficinaCET107",
                        $"<h2>Bem-vindo à OficinaCET107</h2>" +
                        $"<p>A sua conta foi criada.</p>" +
                        $"<p>Para definir a sua password, clique no link abaixo:</p>" +
                        $"<a href=\"{link}\">Definir Password</a>");

                    if (!response.IsSuccess)
                    {
                        ModelState.AddModelError(
                            string.Empty,
                            response.Message);

                        return View(model);
                    }

                    ViewBag.Message =
                        "Utilizador criado com sucesso. Foi enviado um email para definir a password.";

                    return View(new CreateUserViewModel());
                }

                ModelState.AddModelError(
                    string.Empty,
                    result.Errors.FirstOrDefault()?.Description
                    ?? "Erro ao criar o utilizador.");
            }

            return View(model);
        }

    }
}
