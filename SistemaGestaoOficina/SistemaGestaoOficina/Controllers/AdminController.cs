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

        public AdminController(
            IUserHelper userHelper,
            IMailHelper mailHelper)
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
                var userByEmail =
                    await _userHelper.GetUserByEmailAsync(model.Email);

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


                // Se informou NIF, verificar se já existe
                if (!string.IsNullOrWhiteSpace(model.NIF))
                {
                    var userByNif = _userHelper
                        .GetUsers()
                        .FirstOrDefault(u => u.NIF == model.NIF);

                    if (userByNif != null)
                    {
                        ModelState.AddModelError(
                            nameof(model.NIF),
                            "Já existe um utilizador com este NIF.");

                        return View(model);
                    }
                }


                var user = new User
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    UserName = model.Email,
                    PhoneNumber = model.PhoneNumber,

                    NIF = model.Role == "Cliente"
                        ? model.NIF
                        : null
                };


                var result =
                    await _userHelper.AddUserAsync(user, "Temp123!");


                if (result.Succeeded)
                {
                    await _userHelper.AddUserToRoleAsync(
                        user,
                        model.Role);


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


        public async Task<IActionResult> Funcionarios()
        {
            var funcionarios =
                await _userHelper.GetUsersInRoleAsync("Funcionario");

            return View(funcionarios);
        }


        public async Task<IActionResult> Clientes()
        {
            var clientes =
                await _userHelper.GetUsersInRoleAsync("Cliente");

            return View(clientes);
        }


        public async Task<IActionResult> EditUser(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }


            var user = await _userHelper.GetUserByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }


            var model = new EditUserViewModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email!,
                PhoneNumber = user.PhoneNumber!,
                NIF = user.NIF
            };


            if (await _userHelper.IsUserInRoleAsync(user, "Cliente"))
            {
                model.Role = "Cliente";
            }
            else if (await _userHelper.IsUserInRoleAsync(user, "Funcionario"))
            {
                model.Role = "Funcionario";
            }


            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(EditUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }


            var user = await _userHelper.GetUserByIdAsync(model.Id);

            if (user == null)
            {
                return NotFound();
            }


            // Verificar se o email pertence a outro utilizador
            var userByEmail =
                await _userHelper.GetUserByEmailAsync(model.Email);

            if (userByEmail != null && userByEmail.Id != user.Id)
            {
                ModelState.AddModelError(
                    nameof(model.Email),
                    "Já existe um utilizador com este email.");

                return View(model);
            }


            // Verificar se o contacto pertence a outro utilizador
            var userByPhone =
                await _userHelper.GetUserByPhoneNumberAsync(model.PhoneNumber);

            if (userByPhone != null && userByPhone.Id != user.Id)
            {
                ModelState.AddModelError(
                    nameof(model.PhoneNumber),
                    "Já existe um utilizador com este contacto.");

                return View(model);
            }


            // Se informou NIF, verificar se pertence a outro utilizador
            if (!string.IsNullOrWhiteSpace(model.NIF))
            {
                var userByNif = _userHelper
                    .GetUsers()
                    .FirstOrDefault(u =>
                        u.NIF == model.NIF &&
                        u.Id != user.Id);

                if (userByNif != null)
                {
                    ModelState.AddModelError(
                        nameof(model.NIF),
                        "Já existe um utilizador com este NIF.");

                    return View(model);
                }
            }


            // Atualizar dados do User
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Email = model.Email;
            user.UserName = model.Email;
            user.PhoneNumber = model.PhoneNumber;

            user.NIF = model.Role == "Cliente"
                ? model.NIF
                : null;


            var result = await _userHelper.UpdateUserAsync(user);


            if (!result.Succeeded)
            {
                ModelState.AddModelError(
                    string.Empty,
                    result.Errors.FirstOrDefault()?.Description
                    ?? "Erro ao atualizar o utilizador.");

                return View(model);
            }


            if (model.Role == "Cliente")
            {
                return RedirectToAction(nameof(Clientes));
            }


            return RedirectToAction(nameof(Funcionarios));
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }


            var user = await _userHelper.GetUserByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }


            bool isCliente =
                await _userHelper.IsUserInRoleAsync(user, "Cliente");


            var result =
                await _userHelper.DeleteUserAsync(user);


            if (!result.Succeeded)
            {
                TempData["ErrorMessage"] =
                    "Não foi possível apagar o utilizador.";

                return isCliente
                    ? RedirectToAction(nameof(Clientes))
                    : RedirectToAction(nameof(Funcionarios));
            }


            TempData["SuccessMessage"] =
                "Utilizador apagado com sucesso.";


            return isCliente
                ? RedirectToAction(nameof(Clientes))
                : RedirectToAction(nameof(Funcionarios));
        }
    }
}