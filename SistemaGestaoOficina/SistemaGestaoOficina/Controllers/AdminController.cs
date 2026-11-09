using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaGestaoOficina.Data.Entities;
using SistemaGestaoOficina.Data.Repository;
using SistemaGestaoOficina.Helpers;
using SistemaGestaoOficina.Models;

namespace SistemaGestaoOficina.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IUserHelper _userHelper;
        private readonly IMailHelper _mailHelper;
        private readonly IBlobHelper _blobHelper;

        private readonly IMecanicoRepository _mecanicoRepository;
        private readonly ITipoReparacaoRepository _tipoReparacaoRepository;

        public AdminController(IUserHelper userHelper, 
            IMailHelper mailHelper, 
            IBlobHelper blobHelper, 
            ITipoReparacaoRepository tipoReparacaoRepository,
            IMecanicoRepository mecanicoRepository)
        {
            _userHelper = userHelper;
            _mailHelper = mailHelper;
            _blobHelper = blobHelper;
            _tipoReparacaoRepository = tipoReparacaoRepository;
            _mecanicoRepository = mecanicoRepository;
        }


        public async Task<IActionResult> Index()
        {
            // Buscar os rececionistas registados
            var rececionistas = await _userHelper.GetUsersInRoleAsync("Rececionista");

            // Quantidade de rececionistas
            ViewBag.TotalRececionistas = rececionistas.Count();

            // Lista de rececionistas
            ViewBag.Rececionistas = rececionistas;

            // Quantidade total de utilizadores
            ViewBag.TotalUtilizadores = _userHelper.GetUsers().Count();

            // Mecânicos registados
            var mecanicos = _mecanicoRepository.GetAll().ToList();

            ViewBag.TotalMecanicos = mecanicos.Count;
            ViewBag.Mecanicos = mecanicos;

            // Quantidade de tipos de reparações
            ViewBag.TotalTiposReparacoes = _tipoReparacaoRepository.GetAll().Count();

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
            // O Administrador só pode criar Rececionistas por enquanto
            if (model.Role != "Rececionista")
            {
                ModelState.AddModelError(
                    nameof(model.Role),
                    "O Administrador só pode criar contas de funcionários.");
            }

            // Validar a fotografia
            if (model.ImageFile != null)
            {
                var extensao = Path.GetExtension(model.ImageFile.FileName).ToLowerInvariant();

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

            if (ModelState.IsValid)
            {
                // Verificar se o email já existe
                var userByEmail = await _userHelper.GetUserByEmailAsync(model.Email);

                if (userByEmail != null)
                {
                    ModelState.AddModelError(
                        nameof(model.Email),
                        "Já existe um utilizador com este email.");

                    return View(model);
                }

                // Verificar se o contacto já existe
                var userByPhone = await _userHelper.GetUserByPhoneNumberAsync(model.PhoneNumber);

                if (userByPhone != null)
                {
                    ModelState.AddModelError(
                        nameof(model.PhoneNumber),
                        "Já existe um utilizador com este contacto.");

                    return View(model);
                }

                // Criar o Rececionista
                var user = new User
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    UserName = model.Email,
                    PhoneNumber = model.PhoneNumber
                };

                // Guardar fotografia, caso exista
                if (model.ImageFile != null)
                {
                    user.ImageId = await _blobHelper.UploadBlobAsync(
                        model.ImageFile, "profile");
                }

                // Criar a conta com password temporária
                var result = await _userHelper.AddUserAsync(user, "Temp123!");

                if (result.Succeeded)
                {
                    // Atribuir a Role de Rececionista
                    await _userHelper.AddUserToRoleAsync(user, "Rececionista");

                    // Gerar token para definir a password
                    var token = await _userHelper.GeneratePasswordResetTokenAsync(user);

                    var link = Url.Action(
                        "ResetPassword",
                        "Account",
                        new
                        {
                            userName = user.Email,
                            token = token
                        },
                        protocol: HttpContext.Request.Scheme);

                    // Enviar email ao novo Rececionista
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

                    // Voltar à lista de Rececionistas
                    return RedirectToAction(nameof(Rececionistas));
                }

                ModelState.AddModelError(
                    string.Empty,
                    result.Errors.FirstOrDefault()?.Description
                    ?? "Erro ao criar o rececionista.");
            }

            return View(model);
        }


        public async Task<IActionResult> Rececionistas()
        {
            var rececionistas = await _userHelper.GetUsersInRoleAsync("Rececionista");

            return View(rececionistas);
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

            // O Administrador só pode editar Rececionistas
            if (!await _userHelper.IsUserInRoleAsync(user, "Rececionista"))
            {
                return Forbid();
            }

            var model = new EditUserViewModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email!,
                PhoneNumber = user.PhoneNumber!,
                Role = "Rececionista"
            };

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

            // O Admin só pode editar Rececionistas
            if (!await _userHelper.IsUserInRoleAsync(user, "Rececionista"))
            {
                return Forbid();
            }

            // Verificar se o email pertence a outro utilizador
            var userByEmail =
                await _userHelper.GetUserByEmailAsync(model.Email);

            if (userByEmail != null && userByEmail.Id != user.Id)
            {
                ModelState.AddModelError(nameof(model.Email),"Já existe um utilizador com este email.");

                return View(model);
            }

            // Verificar se o contacto pertence a outro utilizador
            var userByPhone = await _userHelper.GetUserByPhoneNumberAsync(model.PhoneNumber);

            if (userByPhone != null && userByPhone.Id != user.Id)
            {
                ModelState.AddModelError(nameof(model.PhoneNumber), "Já existe um utilizador com este contacto.");

                return View(model);
            }

            // Atualizar dados do Rececionista
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Email = model.Email;
            user.UserName = model.Email;
            user.PhoneNumber = model.PhoneNumber;

            var result = await _userHelper.UpdateUserAsync(user);

            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, result.Errors.FirstOrDefault()?.Description ?? "Erro ao atualizar o rececionista.");

                return View(model);
            }

            return RedirectToAction(nameof(Rececionistas));
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


            // O Admin só pode apagar Rececionistas
            if (!await _userHelper.IsUserInRoleAsync(user, "Rececionista"))
            {
                return Forbid();
            }

            var result = await _userHelper.DeleteUserAsync(user);

            if (!result.Succeeded)
            {
                TempData["ErrorMessage"] = "Não foi possível apagar o rececionista.";
                return RedirectToAction(nameof(Rececionistas));
            }

            TempData["SuccessMessage"] = "Rececionista apagado com sucesso.";

            return RedirectToAction(nameof(Rececionistas));
        }

    }
}