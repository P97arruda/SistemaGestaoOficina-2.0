
using System.Net.Http.Headers;
using System.Net.Http.Json;
using OficinaCET107.Mobile.Models;

namespace OficinaCET107.Mobile.Services
{
    public class VeiculoService
    {
        private readonly HttpClient _httpClient;
        private readonly AuthService _authService;

        public VeiculoService(
            HttpClient httpClient,
            AuthService authService)
        {
            _httpClient = httpClient;
            _authService = authService;
        }

        public async Task<List<Veiculo>> GetMeusVeiculosAsync()
        {
            // Recuperar o token guardado no Login
            var token = await _authService.GetTokenAsync();

            if (string.IsNullOrWhiteSpace(token))
            {
                throw new UnauthorizedAccessException(
                    "Cliente não autenticado.");
            }

            // Preparar a requisição
            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                "api/mobile/cliente/veiculos");

            // Enviar o token Bearer
            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            // Chamar a API
            using var response =
                await _httpClient.SendAsync(request);

            response.EnsureSuccessStatusCode();

            // Converter o JSON em lista de veículos
            var veiculos = await response.Content
                .ReadFromJsonAsync<List<Veiculo>>();

            return veiculos ?? new List<Veiculo>();
        }



        public async Task<Veiculo?> GetDetalhesVeiculoAsync(int id)
        {
            // Recuperar o token do Cliente
            var token = await _authService.GetTokenAsync();

            if (string.IsNullOrWhiteSpace(token))
            {
                throw new UnauthorizedAccessException(
                    "Cliente não autenticado.");
            }

            // Preparar a requisição
            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"api/mobile/cliente/veiculos/{id}");

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            // Consultar a API
            using var response = await _httpClient.SendAsync(request);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();

            // Converter o JSON para o modelo Veiculo
            return await response.Content.ReadFromJsonAsync<Veiculo>();
        }

    }
}
