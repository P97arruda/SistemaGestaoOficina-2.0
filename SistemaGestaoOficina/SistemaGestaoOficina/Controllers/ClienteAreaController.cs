using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SistemaGestaoOficina.Data;
using SistemaGestaoOficina.Data.Entities;
using SistemaGestaoOficina.Models;

namespace SistemaGestaoOficina.Controllers
{
    [Authorize(Roles = "Cliente")]
    public class ClienteAreaController : Controller
    {
        private readonly IVeiculoRepository _veiculoRepository;
        private readonly UserManager<User> _userManager;

        public ClienteAreaController(IVeiculoRepository veiculoRepository, UserManager<User> userManager)
        {
            _veiculoRepository = veiculoRepository;
            _userManager = userManager;
        }

        


        public async Task<IActionResult> Index()
        {
            var cliente = await _userManager.GetUserAsync(User);

            if (cliente == null)
            {
                return NotFound();
            }

            var veiculos = _veiculoRepository.GetAll()
                .Where(v => v.UserId == cliente.Id)
                .ToList();

            return View(veiculos);
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
    }
}