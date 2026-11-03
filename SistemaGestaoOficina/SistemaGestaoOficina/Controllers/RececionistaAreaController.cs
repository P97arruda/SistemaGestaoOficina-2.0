using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaGestaoOficina.Data;
using SistemaGestaoOficina.Data.Entities;
using SistemaGestaoOficina.Helpers;
using SistemaGestaoOficina.Models;
using Microsoft.EntityFrameworkCore;


namespace SistemaGestaoOficina.Controllers
{
    [Authorize(Roles = "Rececionista")]
    public class RececionistaAreaController : Controller
    {
        private readonly IUserHelper _userHelper;
        private readonly IMailHelper _mailHelper;

        private readonly IVeiculoRepository _veiculoRepository;

        public RececionistaAreaController(IUserHelper userHelper, IMailHelper mailHelper, IVeiculoRepository veiculoRepository)
        {
            _userHelper = userHelper;
            _mailHelper = mailHelper;
            _veiculoRepository = veiculoRepository;
        }

        public async Task<IActionResult> Index()
        {
            // Contar os clientes
            var clientes = await _userHelper.GetUsersInRoleAsync("Cliente");

            ViewBag.TotalClientes = clientes.Count();

            // Contar os veículos
            ViewBag.TotalVeiculos = await _veiculoRepository.GetAll().CountAsync();

            // Buscar os últimos 5 veículos
            var ultimosVeiculos = await _veiculoRepository.GetAllWithUsers().OrderByDescending(v => v.Id).Take(5).ToListAsync();

            ViewBag.UltimosVeiculos = ultimosVeiculos;

            return View();
        }


        public async Task<IActionResult> Clientes()
        {
            var clientes = await _userHelper.GetUsersInRoleAsync("Cliente");

            return View(clientes);
        }


        public IActionResult CreateCliente()
        {
            var model = new CreateClienteViewModel();

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCliente(CreateClienteViewModel model)
        {

            if (ModelState.IsValid)
            {
                // Verificar se já existe um utilizador com este email
                var userByEmail = await _userHelper.GetUserByEmailAsync(model.Email);

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



        public async Task<IActionResult> EditCliente(string id)
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

            // Garantir que o utilizador é realmente um Cliente
            if (!await _userHelper.IsUserInRoleAsync(user, "Cliente"))
            {
                return Forbid();
            }

            var model = new EditClienteViewModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email!,
                PhoneNumber = user.PhoneNumber!,
                NIF = user.NIF
            };

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCliente(EditClienteViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userHelper.GetUserByIdAsync(model.Id);

                if (user == null)
                {
                    return NotFound();
                }

                // Garantir que o utilizador é realmente um Cliente
                if (!await _userHelper.IsUserInRoleAsync(user, "Cliente"))
                {
                    return Forbid();
                }

                // Atualizar os dados do Cliente
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.Email = model.Email;
                user.UserName = model.Email;
                user.PhoneNumber = model.PhoneNumber;
                user.NIF = model.NIF;

                var result = await _userHelper.UpdateUserAsync(user);

                if (result.Succeeded)
                {
                    return RedirectToAction(nameof(Clientes));
                }

                ModelState.AddModelError(string.Empty, result.Errors.FirstOrDefault()?.Description ?? "Erro ao atualizar o cliente.");
            }

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCliente(string id)
        {
            if(string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userHelper.GetUserByIdAsync(id);

            if(user == null)
            {
                return NotFound();
            }

            // Garantir que o utilizador é realmente um Cliente

            if (!await _userHelper.IsUserInRoleAsync(user, "Cliente"))
            {
                return Forbid();
            }

            var result = await _userHelper.DeleteUserAsync(user);

            if (!result.Succeeded)
            {
                TempData["ErrorMessage"] = "Não foi possível apagar o cliente.";
            }

            return RedirectToAction(nameof(Clientes));
        }
    }
}
