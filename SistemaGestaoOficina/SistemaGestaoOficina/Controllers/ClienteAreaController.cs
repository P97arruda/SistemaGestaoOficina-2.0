using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaGestaoOficina.Data;
using SistemaGestaoOficina.Data.Entities;
using SistemaGestaoOficina.Data.Repository;
using SistemaGestaoOficina.Helpers;
using SistemaGestaoOficina.Models;

namespace SistemaGestaoOficina.Controllers
{
    [Authorize(Roles = "Cliente")]
    public class ClienteAreaController : Controller
    {
        private readonly IVeiculoRepository _veiculoRepository;
        private readonly IMarcacaoRepository _marcacaoRepository;
        private readonly IReparacaoRepository _reparacaoRepository;

        private readonly IBlobHelper _blobHelper;
        private readonly UserManager<User> _userManager;

        public ClienteAreaController(IVeiculoRepository veiculoRepository, 
            UserManager<User> userManager, IBlobHelper blobHelper, 
            IMarcacaoRepository marcacaoRepository, 
            IReparacaoRepository reparacaoRepository)
        {
            _veiculoRepository = veiculoRepository;
            _userManager = userManager;
            _blobHelper = blobHelper;
            _marcacaoRepository = marcacaoRepository;
            _reparacaoRepository = reparacaoRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var cliente = await _userManager.GetUserAsync(User);

            if (cliente == null)
            {
                return NotFound();
            }

            // Contar apenas os veículos do cliente autenticado
            ViewBag.TotalVeiculos = await _veiculoRepository
                .GetAll()
                .CountAsync(v => v.UserId == cliente.Id);

            // Mostrar apenas as marcações pertencentes ao cliente
            var marcacoes = await _marcacaoRepository
                .GetAll()
                .AsNoTracking()
                .Include(m => m.Veiculo)
                .Include(m => m.Mecanico)
                .Where(m =>
                    m.UserId == cliente.Id &&
                    m.Veiculo != null &&
                    m.Veiculo.UserId == cliente.Id)
                .OrderByDescending(m => m.DataHora)
                .ToListAsync();

            return View(marcacoes);
        }

        [HttpGet]
        public async Task<IActionResult> MeusVeiculos()
        {
            var cliente = await _userManager.GetUserAsync(User);

            if (cliente == null)
            {
                return NotFound();
            }

            var veiculos = _veiculoRepository.GetAll()
                .Where(v => v.UserId == cliente.Id)
                .OrderBy(v => v.Marca)
                .ThenBy(v => v.Modelo)
                .ToList();

            return View(veiculos);
        }


        [HttpGet]
        public async Task<IActionResult> DetalhesMarcacao(int id)
        {
            var cliente = await _userManager.GetUserAsync(User);

            if (cliente == null)
            {
                return NotFound();
            }

            var marcacao = await _marcacaoRepository
                .GetAll()
                .AsNoTracking()
                .Include(m => m.User)
                .Include(m => m.Veiculo)
                .Include(m => m.Mecanico)
                .FirstOrDefaultAsync(m =>
                    m.Id == id &&
                    m.UserId == cliente.Id &&
                    m.Veiculo != null &&
                    m.Veiculo.UserId == cliente.Id);

            if (marcacao == null)
            {
                return NotFound();
            }

            return View(marcacao);
        }


        // GET: ClienteArea/DetalhesVeiculo/5
        [HttpGet]
        public async Task<IActionResult> DetalhesVeiculo(int id)
        {
            var cliente = await _userManager.GetUserAsync(User);

            if (cliente == null)
            {
                return NotFound();
            }

            var veiculo = await _veiculoRepository
                .GetAll()
                .AsNoTracking()
                .Include(v => v.User)
                .FirstOrDefaultAsync(v =>
                    v.Id == id &&
                    v.UserId == cliente.Id);

            if (veiculo == null)
            {
                return NotFound();
            }

            return View(veiculo);
        }

        [HttpGet]
        public async Task<IActionResult> EditarVeiculo(int id)
        {
            var cliente = await _userManager.GetUserAsync(User);

            if (cliente == null)
            {
                return NotFound();
            }

            var veiculo = await _veiculoRepository.GetByIdAsync(id);

            if (veiculo == null || veiculo.UserId != cliente.Id)
            {
                return NotFound();
            }

            var model = new VeiculoViewModel
            {
                Id = veiculo.Id,
                UserId = veiculo.UserId,
                Marca = veiculo.Marca,
                Modelo = veiculo.Modelo,
                Matricula = veiculo.Matricula,
                Ano = veiculo.Ano,
                Quilometragem = veiculo.Quilometragem,
                Combustivel = veiculo.Combustivel
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarVeiculo(VeiculoViewModel model)
        {
            var cliente = await _userManager.GetUserAsync(User);

            if (cliente == null)
            {
                return NotFound();
            }

            var veiculo = await _veiculoRepository.GetByIdAsync(model.Id);

            if (veiculo == null || veiculo.UserId != cliente.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                veiculo.Marca = model.Marca;
                veiculo.Modelo = model.Modelo;
                veiculo.Matricula = model.Matricula;
                veiculo.Ano = model.Ano;
                veiculo.Quilometragem = model.Quilometragem;
                veiculo.Combustivel = model.Combustivel;

                await _veiculoRepository.UpdateAsync(veiculo);

                return RedirectToAction(nameof(Index));
            }

            model.UserId = veiculo.UserId;

            return View(model);
        }


        [HttpGet]
        public async Task<IActionResult> EditarPerfil()
        {
            var cliente = await _userManager.GetUserAsync(User);

            if (cliente == null)
            {
                return NotFound();
            }

            var model = new EditarPerfilViewModel
            {
                FirstName = cliente.FirstName,
                LastName = cliente.LastName,
                NIF = cliente.NIF,
                Email = cliente.Email ?? string.Empty,
                PhoneNumber = cliente.PhoneNumber,

                ImageFullPath = cliente.ImageId != Guid.Empty
                    ? cliente.ImageFullPath
                    : null
            };

            return View(model);
        }


        // POST: ClienteArea/EditarPerfil
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarPerfil(EditarPerfilViewModel model)
        {
            var cliente = await _userManager.GetUserAsync(User);

            if (cliente == null)
            {
                return NotFound();
            }

            // O email não pode ser alterado nesta página
            model.Email = cliente.Email ?? string.Empty;
            ModelState.Remove(nameof(EditarPerfilViewModel.Email));

            // Manter a fotografia atual
            model.ImageFullPath = cliente.ImageId != Guid.Empty
                ? cliente.ImageFullPath
                : null;

            // Validar a nova fotografia
            if (model.ImageFile != null)
            {
                var extensao = Path.GetExtension(
                    model.ImageFile.FileName).ToLowerInvariant();

                var extensoesPermitidas = new[]
                {
                    ".jpg",
                    ".jpeg",
                    ".png",
                    ".webp"
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
                        "A fotografia deve ter até 2 MB.");
                }
            }

            if (ModelState.IsValid)
            {
                // Atualizar os dados pessoais
                cliente.FirstName = model.FirstName;
                cliente.LastName = model.LastName;
                cliente.NIF = model.NIF;
                cliente.PhoneNumber = model.PhoneNumber;

                // Atualizar fotografia apenas se foi escolhida uma nova
                if (model.ImageFile != null)
                {
                    var imageId = await _blobHelper.UploadBlobAsync(
                        model.ImageFile,
                        "profile");

                    cliente.ImageId = imageId;
                }

                // Guardar as alterações do cliente
                var result = await _userManager.UpdateAsync(cliente);

                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] =
                        "Perfil atualizado com sucesso!";

                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return View(model);
        }


        // GET: ClienteArea/AlterarPassword
        [HttpGet]
        public IActionResult AlterarPassword()
        {
            return View(new ChangePasswordViewModel());
        }

        // POST: ClienteArea/AlterarPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AlterarPassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var cliente = await _userManager.GetUserAsync(User);

            if (cliente == null)
            {
                return NotFound();
            }

            var result = await _userManager.ChangePasswordAsync(
                cliente,
                model.OldPassword,
                model.NewPassword);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                return View(model);
            }

            TempData["SuccessMessage"] = "Password alterada com sucesso!";

            return RedirectToAction(nameof(EditarPerfil));
        }

        // GET: ClienteArea/HistoricoReparacoes
        [HttpGet]
        public async Task<IActionResult> HistoricoReparacoes()
        {
            var cliente = await _userManager.GetUserAsync(User);

            if (cliente == null)
            {
                return NotFound();
            }

            // Apenas reparações concluídas do cliente autenticado
            var reparacoes = await _reparacaoRepository
                .GetAll()
                .AsNoTracking()
                .Include(r => r.Marcacao)
                    .ThenInclude(m => m.Veiculo)
                .Where(r =>
                    r.Concluida &&
                    !r.Cancelada &&
                    r.Marcacao != null &&
                    r.Marcacao.UserId == cliente.Id &&
                    r.Marcacao.Veiculo != null &&
                    r.Marcacao.Veiculo.UserId == cliente.Id)
                .OrderByDescending(r => r.DataInicio)
                .ToListAsync();

            return View(reparacoes);
        }


        // GET: ClienteArea/DetalhesReparacao/5
        [HttpGet]
        public async Task<IActionResult> DetalhesReparacao(int id)
        {
            var cliente = await _userManager.GetUserAsync(User);

            if (cliente == null)
            {
                return NotFound();
            }

            // Procurar apenas uma reparação pertencente ao cliente
            var reparacao = await _reparacaoRepository
                .GetAll()
                .AsNoTracking()
                .Include(r => r.Marcacao)
                    .ThenInclude(m => m.Veiculo)
                .Include(r => r.ReparacaoPecas)
                    .ThenInclude(rp => rp.Peca)
                .FirstOrDefaultAsync(r =>
                    r.Id == id &&
                    r.Concluida &&
                    !r.Cancelada &&
                    r.Marcacao != null &&
                    r.Marcacao.UserId == cliente.Id &&
                    r.Marcacao.Veiculo != null &&
                    r.Marcacao.Veiculo.UserId == cliente.Id);

            if (reparacao == null)
            {
                return NotFound();
            }

            return View(reparacao);
        }
    }
}
