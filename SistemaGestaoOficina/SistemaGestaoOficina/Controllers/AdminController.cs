
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
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

        private readonly IReparacaoRepository _reparacaoRepository;

        public AdminController(
            IUserHelper userHelper,
            IMailHelper mailHelper,
            IBlobHelper blobHelper,
            ITipoReparacaoRepository tipoReparacaoRepository,
            IMecanicoRepository mecanicoRepository,
            IReparacaoRepository reparacaoRepository)
        {
            _userHelper = userHelper;
            _mailHelper = mailHelper;
            _blobHelper = blobHelper;
            _tipoReparacaoRepository = tipoReparacaoRepository;
            _mecanicoRepository = mecanicoRepository;
            _reparacaoRepository = reparacaoRepository;
        }

        public async Task<IActionResult> Index()
        {
            // Últimos 12 meses, incluindo o mês atual
            var mesAtual = new DateTime(
                DateTime.Today.Year,
                DateTime.Today.Month,
                1);

            var inicio = mesAtual.AddMonths(-11);
            var fim = mesAtual.AddMonths(1);

            // Somar os valores das reparações concluídas
            var valoresPorMes = await _reparacaoRepository
                .GetAll()
                .Where(r =>
                    r.Concluida &&
                    !r.Cancelada &&
                    r.DataFim.HasValue &&
                    r.DataFim.Value >= inicio &&
                    r.DataFim.Value < fim)
                .GroupBy(r => new
                {
                    Ano = r.DataFim!.Value.Year,
                    Mes = r.DataFim.Value.Month
                })
                .Select(grupo => new
                {
                    grupo.Key.Ano,
                    grupo.Key.Mes,
                    Total = grupo.Sum(r => r.CustoTotal)
                })
                .ToListAsync();

            var meses = Enumerable.Range(0, 12)
                .Select(i => inicio.AddMonths(i))
                .ToArray();

            var cultura =
                System.Globalization.CultureInfo.GetCultureInfo("pt-PT");

            ViewBag.MesesLabels = meses
                .Select(m => m.ToString("MMM/yy", cultura))
                .ToArray();

            ViewBag.GanhosMensais = meses
                .Select(m =>
                    valoresPorMes.FirstOrDefault(v =>
                        v.Ano == m.Year &&
                        v.Mes == m.Month)?.Total ?? 0m)
                .ToArray();

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> DetailsUser(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            var user = await _userHelper.GetUserByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // GET: Admin/CreateUser
        public IActionResult CreateUser()
        {
            ViewBag.TiposReparacao = new SelectList(
                _tipoReparacaoRepository.GetAll()
                    .OrderBy(t => t.Nome),
                "Id",
                "Nome");

            return View(new CreateUserViewModel());
        }

        // POST: Admin/CreateUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(
            CreateUserViewModel model)
        {
            ViewBag.TiposReparacao = new SelectList(
                _tipoReparacaoRepository.GetAll()
                    .OrderBy(t => t.Nome),
                "Id",
                "Nome",
                model.TipoReparacaoId);

            // Validar função
            if (model.Role != "Rececionista" &&
                model.Role != "Mecanico")
            {
                ModelState.AddModelError(
                    nameof(model.Role),
                    "Selecione uma função válida: Rececionista ou Mecânico.");
            }

            // Validar especialidade
            if (model.Role == "Mecanico")
            {
                if (!model.TipoReparacaoId.HasValue)
                {
                    ModelState.AddModelError(
                        nameof(model.TipoReparacaoId),
                        "Selecione uma especialidade.");
                }
                else
                {
                    var especialidade =
                        _tipoReparacaoRepository.GetAll()
                            .FirstOrDefault(t =>
                                t.Id == model.TipoReparacaoId.Value);

                    if (especialidade == null)
                    {
                        ModelState.AddModelError(
                            nameof(model.TipoReparacaoId),
                            "A especialidade selecionada não existe.");
                    }
                }
            }

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

            // Verificar email duplicado
            var userByEmail =
                await _userHelper.GetUserByEmailAsync(model.Email);

            if (userByEmail != null)
            {
                ModelState.AddModelError(
                    nameof(model.Email),
                    "Já existe um utilizador com este email.");

                return View(model);
            }

            // Verificar contacto duplicado
            var userByPhone =
                await _userHelper.GetUserByPhoneNumberAsync(
                    model.PhoneNumber);

            if (userByPhone != null)
            {
                ModelState.AddModelError(
                    nameof(model.PhoneNumber),
                    "Já existe um utilizador com este contacto.");

                return View(model);
            }

            // Criar conta Identity
            var user = new User
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                UserName = model.Email,
                PhoneNumber = model.PhoneNumber
            };

            // Guardar fotografia no Azure
            if (model.ImageFile != null)
            {
                user.ImageId = await _blobHelper.UploadBlobAsync(
                    model.ImageFile, "profile");
            }

            var result = await _userHelper.AddUserAsync(
                user, "Temp123!");

            if (!result.Succeeded)
            {
                ModelState.AddModelError(
                    string.Empty,
                    result.Errors.FirstOrDefault()?.Description
                    ?? "Erro ao criar o funcionário.");

                return View(model);
            }

            // Atribuir função
            await _userHelper.AddUserToRoleAsync(
                user, model.Role);

            // Criar registo profissional
            if (model.Role == "Mecanico")
            {
                var tipoReparacao =
                    _tipoReparacaoRepository.GetAll()
                        .First(t =>
                            t.Id == model.TipoReparacaoId!.Value);

                var mecanico = new Mecanico
                {
                    Nome = $"{model.FirstName} {model.LastName}",
                    Especialidade = tipoReparacao.Nome,
                    Horario = "09:00 às 18:00",
                    Contacto = model.PhoneNumber,
                    Ativo = model.Ativo,
                    UserId = user.Id
                };

                await _mecanicoRepository.CreateAsync(mecanico);
            }

            // Gerar link para definir password
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

            if (model.Role == "Mecanico")
                return RedirectToAction("Index", "Mecanicos");

            return RedirectToAction(nameof(Rececionistas));
        }

        // LISTA DE RECECIONISTAS
        public async Task<IActionResult> Rececionistas()
        {
            var rececionistas =
                await _userHelper.GetUsersInRoleAsync("Rececionista");

            return View(rececionistas);
        }

        // GET: Admin/EditUser
        [HttpGet]
        public async Task<IActionResult> EditUser(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var user = await _userHelper.GetUserByIdAsync(id);

            if (user == null)
                return NotFound();

            bool isMecanico = await _userHelper
                .IsUserInRoleAsync(user, "Mecanico");

            bool isRececionista = await _userHelper
                .IsUserInRoleAsync(user, "Rececionista");

            if (!isMecanico && !isRececionista)
                return Forbid();

            var model = new EditUserViewModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email ?? "",
                PhoneNumber = user.PhoneNumber ?? "",
                NIF = user.NIF,
                Role = isMecanico ? "Mecanico" : "Rececionista",

                // Mostrar fotografia atual do Azure
                ImageUrl = user.ImageFullPath
            };

            if (isMecanico)
            {
                var mecanico = _mecanicoRepository.GetAll()
                    .FirstOrDefault(m => m.UserId == user.Id);

                if (mecanico == null)
                    return NotFound();

                var tipoReparacao =
                    _tipoReparacaoRepository.GetAll()
                        .FirstOrDefault(t =>
                            t.Nome == mecanico.Especialidade);

                model.TipoReparacaoId = tipoReparacao?.Id;

                // Carregar estado atual
                model.Ativo = mecanico.Ativo;

                ViewBag.TiposReparacao = new SelectList(
                    _tipoReparacaoRepository.GetAll()
                        .OrderBy(t => t.Nome),
                    "Id",
                    "Nome",
                    model.TipoReparacaoId);
            }

            return View(model);
        }

        // POST: Admin/EditUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(
            EditUserViewModel model)
        {
            var user =
                await _userHelper.GetUserByIdAsync(model.Id);

            if (user == null)
                return NotFound();

            bool isMecanico = await _userHelper
                .IsUserInRoleAsync(user, "Mecanico");

            bool isRececionista = await _userHelper
                .IsUserInRoleAsync(user, "Rececionista");

            if (!isMecanico && !isRececionista)
                return Forbid();

            // A função verdadeira vem do Identity
            model.Role = isMecanico
                ? "Mecanico"
                : "Rececionista";

            ModelState.Remove(nameof(model.Role));

            // Manter a fotografia atual nos erros de validação
            model.ImageUrl = user.ImageFullPath;

            Mecanico? mecanico = null;
            TipoReparacao? especialidade = null;

            if (isMecanico)
            {
                mecanico = _mecanicoRepository.GetAll()
                    .FirstOrDefault(m => m.UserId == user.Id);

                if (mecanico == null)
                    return NotFound();

                if (model.TipoReparacaoId.HasValue)
                {
                    especialidade =
                        _tipoReparacaoRepository.GetAll()
                            .FirstOrDefault(t =>
                                t.Id == model.TipoReparacaoId.Value);
                }

                if (especialidade == null)
                {
                    ModelState.AddModelError(
                        nameof(model.TipoReparacaoId),
                        "Selecione uma especialidade válida.");
                }

                ViewBag.TiposReparacao = new SelectList(
                    _tipoReparacaoRepository.GetAll()
                        .OrderBy(t => t.Nome),
                    "Id",
                    "Nome",
                    model.TipoReparacaoId);

                // Impedir desativação com marcações agendadas
                if (mecanico.Ativo && !model.Ativo)
                {
                    bool temMarcacoesAgendadas =
                        await _mecanicoRepository.GetAll()
                            .Where(m => m.Id == mecanico.Id)
                            .SelectMany(m => m.Marcacaos)
                            .AnyAsync(m => m.Estado == "Agendada");

                    if (temMarcacoesAgendadas)
                    {
                        ModelState.AddModelError(
                            nameof(model.Ativo),
                            "Não é possível desativar este mecânico porque possui marcações agendadas.");
                    }
                }
            }

            // Validar nova fotografia
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

            // Verificar email duplicado
            var userByEmail = await _userHelper
                .GetUserByEmailAsync(model.Email);

            if (userByEmail != null &&
                userByEmail.Id != user.Id)
            {
                ModelState.AddModelError(
                    nameof(model.Email),
                    "Já existe um utilizador com este email.");

                return View(model);
            }

            // Verificar contacto duplicado
            var userByPhone = await _userHelper
                .GetUserByPhoneNumberAsync(model.PhoneNumber);

            if (userByPhone != null &&
                userByPhone.Id != user.Id)
            {
                ModelState.AddModelError(
                    nameof(model.PhoneNumber),
                    "Já existe um utilizador com este contacto.");

                return View(model);
            }

            // Atualizar dados Identity
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Email = model.Email;
            user.UserName = model.Email;
            user.PhoneNumber = model.PhoneNumber;

            // Guardar nova fotografia no Azure Blob Storage
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
                        "Não foi possível guardar a fotografia no Azure. Tente novamente.");

                    return View(model);
                }
            }

            // Guardar alterações no Identity
            var result = await _userHelper.UpdateUserAsync(user);

            if (!result.Succeeded)
            {
                ModelState.AddModelError(
                    string.Empty,
                    result.Errors.FirstOrDefault()?.Description
                    ?? "Erro ao atualizar o funcionário.");

                return View(model);
            }

            // Atualizar registo profissional
            if (isMecanico &&
                mecanico != null &&
                especialidade != null)
            {
                mecanico.Nome =
                    $"{model.FirstName} {model.LastName}";

                mecanico.Contacto = model.PhoneNumber;
                mecanico.Especialidade = especialidade.Nome;

                // Guardar novo estado
                mecanico.Ativo = model.Ativo;

                await _mecanicoRepository.UpdateAsync(mecanico);
            }

            TempData["SuccessMessage"] =
                "Funcionário atualizado com sucesso.";

            if (isMecanico)
                return RedirectToAction("Index", "Mecanicos");

            return RedirectToAction(nameof(Rececionistas));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return NotFound();

            var user = await _userHelper.GetUserByIdAsync(id);

            if (user == null)
                return NotFound();

            bool isMecanico = await _userHelper
                .IsUserInRoleAsync(user, "Mecanico");

            bool isRececionista = await _userHelper
                .IsUserInRoleAsync(user, "Rececionista");

            if (!isMecanico && !isRececionista)
                return Forbid();

            if (isMecanico)
            {
                var mecanico = await _mecanicoRepository.GetAll()
                    .Include(m => m.Marcacaos)
                    .Include(m => m.MecanicoReparacaoes)
                    .FirstOrDefaultAsync(m => m.UserId == user.Id);

                if (mecanico == null)
                    return NotFound();

                // Impedir eliminação se tiver marcações ou reparações
                if ((mecanico.Marcacaos?.Any() ?? false) ||
                    (mecanico.MecanicoReparacaoes?.Any() ?? false))
                {
                    TempData["ErrorMessage"] =
                        "Não é possível apagar este mecânico porque possui marcações ou histórico de reparações.";

                    return RedirectToAction("Index", "Mecanicos");
                }

                // Eliminar o registo profissional
                await _mecanicoRepository.DeleteAsync(mecanico);
            }

            // Eliminar a conta Identity
            var result = await _userHelper.DeleteUserAsync(user);

            if (!result.Succeeded)
            {
                TempData["ErrorMessage"] =
                    "Não foi possível apagar o funcionário.";

                return isMecanico
                    ? RedirectToAction("Index", "Mecanicos")
                    : RedirectToAction(nameof(Rececionistas));
            }

            TempData["SuccessMessage"] =
                isMecanico
                    ? "Mecânico apagado com sucesso."
                    : "Rececionista apagado com sucesso.";

            return isMecanico
                ? RedirectToAction("Index", "Mecanicos")
                : RedirectToAction(nameof(Rececionistas));
        }
    }
}
