using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaGestaoOficina.Data.Entities;
using SistemaGestaoOficina.Helpers;
using SistemaGestaoOficina.Models;

namespace SistemaGestaoOficina.Controllers
{
    [Authorize(Roles = "Funcionario")]
    public class FuncionarioAreaController : Controller
    {
        private readonly IUserHelper _userHelper;
        private readonly IMailHelper _mailHelper;

        public FuncionarioAreaController(IUserHelper userHelper, IMailHelper mailHelper)
        {
            _userHelper = userHelper;
            _mailHelper = mailHelper;
        }
        
        public IActionResult Index()
        {
            return View();
        }


        public async Task<IActionResult> Clientes()
        {
            var clientes = await _userHelper.GetUsersInRoleAsync("Cliente");

            return View(clientes);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCliente(CreateUserViewModel model)
        {
            // O Funcionário só pode criar Clientes
            model.Role = "Cliente";

            if (ModelState.IsValid)
            {
                // Verificar se já existe um utilizador com este email
                var userByEmail =
                    await _userHelper.GetUserByEmailAsync(model.Email);

                if (userByEmail != null)
                {
                    ModelState.AddModelError(
                        nameof(model.Email),
                        "Já existe um utilizador com este email.");

                    return View(model);
                }

                // Verificar se já existe um utilizador com este contacto
                var userByPhone =
                    await _userHelper.GetUserByPhoneNumberAsync(model.PhoneNumber);

                if (userByPhone != null)
                {
                    ModelState.AddModelError(
                        nameof(model.PhoneNumber),
                        "Já existe um utilizador com este contacto.");

                    return View(model);
                }

                // Verificar se já existe um utilizador com este NIF
                if (!string.IsNullOrWhiteSpace(model.NIF))
                {
                    var userByNif = _userHelper
                        .GetUsers()
                        .FirstOrDefault(u => u.NIF == model.NIF);

                    if (userByNif != null)
                    {
                        ModelState.AddModelError(
                            nameof(model.NIF),
                            "Já existe um cliente com este NIF.");

                        return View(model);
                    }
                }

                // Criar o Cliente
                var user = new User
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    UserName = model.Email,
                    PhoneNumber = model.PhoneNumber,
                    NIF = model.NIF
                };

                var result =
                    await _userHelper.AddUserAsync(user, "Temp123!");

                if (result.Succeeded)
                {
                    // Adicionar à Role Cliente
                    await _userHelper.AddUserToRoleAsync(
                        user,
                        "Cliente");

                    // Gerar token para o Cliente definir a password
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

                    // Enviar email
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

                    return RedirectToAction(nameof(Clientes));
                }

                ModelState.AddModelError(
                    string.Empty,
                    result.Errors.FirstOrDefault()?.Description
                    ?? "Erro ao criar o cliente.");
            }

            return View(model);
        }


    }
}
