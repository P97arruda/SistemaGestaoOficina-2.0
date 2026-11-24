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
    public class VeiculosController : Controller
    {
        private readonly IUserHelper _userHelper;
        private readonly IVeiculoRepository _veiculoRepository;
        private readonly IReparacaoRepository _reparacaoRepository;

        public VeiculosController(IUserHelper userHelper, IVeiculoRepository veiculoRepository, IReparacaoRepository reparacaoRepository)
        {
            _userHelper = userHelper;
            _veiculoRepository = veiculoRepository;
            _reparacaoRepository = reparacaoRepository;
        }


        [HttpGet]
        public async Task<IActionResult> Index(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var cliente = await _userHelper.GetUserByIdAsync(id);

            if (cliente == null)
            {
                return NotFound();
            }

            if (!await _userHelper.IsUserInRoleAsync(cliente, "Cliente"))
            {
                return NotFound();
            }

            var veiculos = _veiculoRepository.GetAll()
                .Where(v => v.UserId == id)
                .ToList();

            ViewBag.ClienteId = id;
            ViewBag.NomeCliente = cliente.FullName;

            return View(veiculos);
        }


        [HttpGet]
        public IActionResult Details(int id)
        {
            var veiculo = _veiculoRepository.GetAllWithUsers()
                .FirstOrDefault(v => v.Id == id);

            if (veiculo == null)
            {
                return NotFound();
            }

            return View(veiculo);
        }


        [HttpGet]
        public async Task<IActionResult> Create(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var cliente = await _userHelper.GetUserByIdAsync(id);

            if (cliente == null ||
                !await _userHelper.IsUserInRoleAsync(cliente, "Cliente"))
            {
                return NotFound();
            }

            var model = new CriarVeiculoViewModel
            {
                UserId = cliente.Id
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CriarVeiculoViewModel model)
        {
            if (string.IsNullOrEmpty(model.UserId))
            {
                return NotFound();
            }

            var cliente = await _userHelper.GetUserByIdAsync(model.UserId);

            if (cliente == null ||
                !await _userHelper.IsUserInRoleAsync(cliente, "Cliente"))
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var veiculo = new Veiculo
            {
                Marca = model.Marca,
                Modelo = model.Modelo,
                Matricula = model.Matricula,
                Ano = model.Ano,
                Quilometragem = model.Quilometragem,
                Combustivel = model.Combustivel,
                UserId = cliente.Id
            };

            await _veiculoRepository.CreateAsync(veiculo);

            return RedirectToAction(nameof(Index), new { id = cliente.Id });
        }


        [HttpGet]
        public IActionResult TodosVeiculos()
        {
            var veiculos = _veiculoRepository.GetAllWithUsers().OrderBy(v => v.Matricula).ToList();

            return View(veiculos);
        }



        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var veiculo = await _veiculoRepository.GetByIdAsync(id);

            if (veiculo == null)
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
        public async Task<IActionResult> Edit(VeiculoViewModel model)
        {
            var veiculo = await _veiculoRepository.GetByIdAsync(model.Id);

            if (veiculo == null)
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

                return RedirectToAction(nameof(Index), new { id = veiculo.UserId });
            }

            model.UserId = veiculo.UserId;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var veiculo = await _veiculoRepository.GetByIdAsync(id);

            if (veiculo == null)
            {
                return NotFound();
            }

            var clienteId = veiculo.UserId;

            await _veiculoRepository.DeleteAsync(veiculo);

            return RedirectToAction(nameof(Index), new { id = clienteId });
        }

        [HttpGet]
        public async Task<IActionResult> HistoricoReparacoes(int id)
        {
            // Procurar o veículo
            var veiculo = await _veiculoRepository.GetByIdAsync(id);

            if (veiculo == null)
            {
                return NotFound();
            }

            // Procurar as reparações associadas ao veículo
            var reparacoes = await _reparacaoRepository.GetAll()
                .Include(r => r.Marcacao)
                .Include(r => r.ReparacaoPecas)
                    .ThenInclude(rp => rp.Peca)
                .Where(r => r.Marcacao != null &&
                            r.Marcacao.Veiculo != null &&
                            r.Marcacao.Veiculo.Id == id &&
                            r.Concluida &&
                            !r.Cancelada)
                .OrderByDescending(r => r.DataInicio)
                .ToListAsync();

            // Enviar o veículo para a View
            ViewBag.Veiculo = veiculo;

            return View(reparacoes);
        }
    }
 }





