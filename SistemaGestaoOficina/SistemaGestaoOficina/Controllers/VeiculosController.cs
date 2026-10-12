using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaGestaoOficina.Data;
using SistemaGestaoOficina.Data.Entities;
using SistemaGestaoOficina.Helpers;
using SistemaGestaoOficina.Models;

namespace SistemaGestaoOficina.Controllers
{
    [Authorize(Roles = "Funcionario")]
    public class VeiculosController : Controller
    {
        private readonly IUserHelper _userHelper;
        private readonly IVeiculoRepository _veiculoRepository;

        public VeiculosController(IUserHelper userHelper, IVeiculoRepository veiculoRepository)
        {
            _userHelper = userHelper;
            _veiculoRepository = veiculoRepository;
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

            return View(veiculos);
        }



        [HttpGet]
        public async Task<IActionResult> Create(string id)
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

            ViewBag.ClienteId = id;

            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VeiculoViewModel model)
        {

            if (string.IsNullOrEmpty(model.UserId))
            {
                return NotFound();
            }

            var cliente = await _userHelper.GetUserByIdAsync(model.UserId);

            if (cliente == null)
            {
                return NotFound();
            }

            if (!await _userHelper.IsUserInRoleAsync(cliente, "Cliente"))
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
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

            // TEMPORÁRIO: mostrar os erros de validação
            foreach (var item in ModelState)
            {
                foreach (var erro in item.Value.Errors)
                {
                    Console.WriteLine($"Campo: {item.Key} | Erro: {erro.ErrorMessage}");
                }
            }

            ViewBag.ClienteId = model.UserId;

            return View(model);
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
    }
 }







