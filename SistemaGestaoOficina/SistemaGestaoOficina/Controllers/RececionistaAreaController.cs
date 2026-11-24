
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaGestaoOficina.Data;
using SistemaGestaoOficina.Data.Entities;
using SistemaGestaoOficina.Data.Repository;
using SistemaGestaoOficina.Helpers;
using SistemaGestaoOficina.Models;

namespace SistemaGestaoOficina.Controllers
{
    [Authorize(Roles = "Rececionista, Mecanico")]
    public class RececionistaAreaController : Controller
    {
        private readonly IUserHelper _userHelper;
        private readonly IMailHelper _mailHelper;
        private readonly IVeiculoRepository _veiculoRepository;
        private readonly IBlobHelper _blobHelper;
        private readonly IMarcacaoRepository _marcacaoRepository;

        public RececionistaAreaController(
            IUserHelper userHelper,
            IMailHelper mailHelper,
            IVeiculoRepository veiculoRepository,
            IBlobHelper blobHelper,
            IMarcacaoRepository marcacaoRepository)
        {
            _userHelper = userHelper;
            _mailHelper = mailHelper;
            _veiculoRepository = veiculoRepository;
            _blobHelper = blobHelper;
            _marcacaoRepository = marcacaoRepository;
        }

        // DASHBOARD

        public async Task<IActionResult> Index()
        {
            // Total de clientes
            var clientes = await _userHelper
                .GetUsersInRoleAsync("Cliente");

            ViewBag.TotalClientes = clientes.Count();

            // Total de veículos
            ViewBag.TotalVeiculos = await _veiculoRepository
                .GetAll()
                .CountAsync();

            // Últimos veículos registados
            var ultimosVeiculos = await _veiculoRepository
                .GetAllWithUsers()
                .OrderByDescending(v => v.Id)
                .Take(5)
                .ToListAsync();

            ViewBag.UltimosVeiculos = ultimosVeiculos;

            // Marcações de hoje
            var hoje = DateTime.Today;
            var amanha = hoje.AddDays(1);

            var marcacoesHoje = await _marcacaoRepository
                .GetAll()
                .Include(m => m.User)
                .Include(m => m.Veiculo)
                .Where(m => m.DataHora >= hoje &&
                            m.DataHora < amanha &&
                            m.Estado != "Cancelada")
                .OrderBy(m => m.DataHora)
                .ToListAsync();

            ViewBag.MarcacoesHoje = marcacoesHoje;

            return View();
        }


        // DETALHES DO CLIENTE
        [HttpGet]
        public async Task<IActionResult> DetailsCliente(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return NotFound();

            var cliente = await _userHelper.GetUserByIdAsync(id);

            if (cliente == null)
                return NotFound();

            if (!await _userHelper.IsUserInRoleAsync(cliente, "Cliente"))
                return NotFound();

            ViewBag.TotalVeiculos = await _veiculoRepository
                .GetAll()
                .CountAsync(v => v.UserId == cliente.Id);

            return View(cliente);
        }

        // LISTAR CLIENTES
        public async Task<IActionResult> Clientes()
        {
            var clientes = await _userHelper
                .GetUsersInRoleAsync("Cliente");

            return View(clientes);
        }

        // CRIAR CLIENTE - GET
        [HttpGet]
        public IActionResult CreateCliente()
        {
            return View(new CreateClienteViewModel());
        }

        // CRIAR CLIENTE - POST

        // CRIAR CLIENTE - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCliente(
            CreateClienteViewModel model)
        {
            // Validar a fotografia, caso tenha sido enviada
            if (model.ImageFile != null)
            {
                var extensao = Path.GetExtension(
                    model.ImageFile.FileName).ToLowerInvariant();

                var extensoesPermitidas = new[]
                {
                    ".jpg", ".jpeg", ".png", ".webp"
                };

                if (!extensoesPermitidas.Contains(extensao))
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
                return View(model);
            }

            // Verificar email duplicado
            var userByEmail = await _userHelper
                .GetUserByEmailAsync(model.Email);

            if (userByEmail != null)
            {
                ModelState.AddModelError(
                    nameof(model.Email),
                    "Já existe um utilizador com este email.");

                return View(model);
            }

            // Verificar contacto duplicado
            var userByPhone = await _userHelper
                .GetUserByPhoneNumberAsync(model.PhoneNumber);

            if (userByPhone != null)
            {
                ModelState.AddModelError(
                    nameof(model.PhoneNumber),
                    "Já existe um utilizador com este contacto.");

                return View(model);
            }

            // Verificar NIF duplicado
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

            // Criar utilizador
            var user = new User
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                UserName = model.Email,
                PhoneNumber = model.PhoneNumber,
                NIF = model.NIF
            };

            // Guardar fotografia no Azure Blob Storage
            if (model.ImageFile != null)
            {
                try
                {
                    user.ImageId = await _blobHelper.UploadBlobAsync(
                        model.ImageFile, "profile");
                }
                catch
                {
                    ModelState.AddModelError(
                        nameof(model.ImageFile),
                        "Não foi possível guardar a fotografia. Tente novamente.");

                    return View(model);
                }
            }

            // Guardar cliente no Identity
            var result = await _userHelper.AddUserAsync(
                user, "Temp123!");

            if (result.Succeeded)
            {
                // Adicionar à Role Cliente
                await _userHelper.AddUserToRoleAsync(
                    user, "Cliente");

                // Gerar token para definir password
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

                TempData["SuccessMessage"] =
                    "Cliente criado com sucesso.";

                return RedirectToAction(nameof(Clientes));
            }

            ModelState.AddModelError(
                string.Empty,
                result.Errors.FirstOrDefault()?.Description
                ?? "Erro ao criar o cliente.");

            return View(model);
        }


        // EDITAR CLIENTE - GET
        [HttpGet]
        public async Task<IActionResult> EditCliente(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return NotFound();

            var user = await _userHelper.GetUserByIdAsync(id);

            if (user == null)
                return NotFound();

            if (!await _userHelper.IsUserInRoleAsync(user, "Cliente"))
                return Forbid();

            var model = new EditClienteViewModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email ?? "",
                PhoneNumber = user.PhoneNumber ?? "",
                NIF = user.NIF,

                // Fotografia atual
                ImageUrl = user.ImageFullPath
            };

            return View(model);
        }

        // EDITAR CLIENTE - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCliente(
            EditClienteViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Id))
                return NotFound();

            var user = await _userHelper.GetUserByIdAsync(model.Id);

            if (user == null)
                return NotFound();

            if (!await _userHelper.IsUserInRoleAsync(user, "Cliente"))
                return Forbid();

            // Manter fotografia atual
            model.ImageUrl = user.ImageFullPath;

            // O email não pode ser alterado nesta edição
            model.Email = user.Email ?? "";
            ModelState.Remove(nameof(model.Email));

            // Validar fotografia
            if (model.ImageFile != null)
            {
                var extensao = Path.GetExtension(
                    model.ImageFile.FileName).ToLowerInvariant();

                var extensoesPermitidas = new[]
                {
                    ".jpg", ".jpeg", ".png", ".webp"
                };

                if (!extensoesPermitidas.Contains(extensao))
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
                return View(model);

            // Verificar contacto duplicado
            var userByPhone = await _userHelper
                .GetUserByPhoneNumberAsync(model.PhoneNumber);

            if (userByPhone != null && userByPhone.Id != user.Id)
            {
                ModelState.AddModelError(
                    nameof(model.PhoneNumber),
                    "Já existe um utilizador com este contacto.");

                return View(model);
            }

            // Verificar NIF duplicado
            if (!string.IsNullOrWhiteSpace(model.NIF))
            {
                var userByNif = _userHelper.GetUsers()
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

            // Guardar nova fotografia no Azure
            if (model.ImageFile != null)
            {
                try
                {
                    user.ImageId = await _blobHelper.UploadBlobAsync(
                        model.ImageFile, "profile");
                }
                catch
                {
                    ModelState.AddModelError(
                        nameof(model.ImageFile),
                        "Não foi possível guardar a fotografia. Tente novamente.");

                    return View(model);
                }
            }

            // Atualizar dados do cliente
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.PhoneNumber = model.PhoneNumber;
            user.NIF = model.NIF;

            // Não alterar Email nem UserName

            var result = await _userHelper.UpdateUserAsync(user);

            if (!result.Succeeded)
            {
                ModelState.AddModelError(
                    string.Empty,
                    result.Errors.FirstOrDefault()?.Description
                    ?? "Erro ao atualizar o cliente.");

                return View(model);
            }

            TempData["SuccessMessage"] =
                "Cliente atualizado com sucesso.";

            return RedirectToAction(nameof(Clientes));
        }

        // APAGAR CLIENTE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCliente(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var user = await _userHelper.GetUserByIdAsync(id);

            if (user == null)
                return NotFound();

            if (!await _userHelper.IsUserInRoleAsync(user, "Cliente"))
                return Forbid();

            var result = await _userHelper.DeleteUserAsync(user);

            if (!result.Succeeded)
            {
                TempData["ErrorMessage"] =
                    "Não foi possível apagar o cliente.";
            }

            return RedirectToAction(nameof(Clientes));
        }
    }
}
