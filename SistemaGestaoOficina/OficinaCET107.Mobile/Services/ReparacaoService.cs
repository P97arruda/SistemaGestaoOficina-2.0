
using System.Net.Http.Headers;
using System.Net.Http.Json;
using OficinaCET107.Mobile.Models;

namespace OficinaCET107.Mobile.Services
{
    public class ReparacaoService
    {
        private readonly HttpClient _httpClient;
        private readonly AuthService _authService;

        public ReparacaoService(
            HttpClient httpClient,
            AuthService authService)
        {
            _httpClient = httpClient;
            _authService = authService;
        }

        // LISTAR HISTÓRICO DE REPARAÇÕES
        public async Task<List<Reparacao>> GetHistoricoReparacoesAsync()
        {
            // Recuperar o token do Cliente
            var token = await _authService.GetTokenAsync();

            if (string.IsNullOrWhiteSpace(token))
            {
                throw new UnauthorizedAccessException(
                    "Cliente não autenticado.");
            }

            // Preparar o pedido à API
            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                "api/mobile/cliente/reparacoes");

            // Enviar o token Bearer
            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            // Consultar a API
            using var response =
                await _httpClient.SendAsync(request);

            response.EnsureSuccessStatusCode();

            // Converter o JSON numa lista de reparações
            var reparacoes = await response.Content
                .ReadFromJsonAsync<List<Reparacao>>();

            return reparacoes ?? new List<Reparacao>();
        }

        // CONSULTAR DETALHES DE UMA REPARAÇÃO
        public async Task<Reparacao> GetDetalhesReparacaoAsync(int id)
        {
            // Recuperar o token do Cliente
            var token = await _authService.GetTokenAsync();

            if (string.IsNullOrWhiteSpace(token))
            {
                throw new UnauthorizedAccessException(
                    "Cliente não autenticado.");
            }

            // Preparar o pedido à API
            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"api/mobile/cliente/reparacoes/{id}");

            // Enviar o token Bearer
            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            // Consultar a API
            using var response =
                await _httpClient.SendAsync(request);

            response.EnsureSuccessStatusCode();

            // Converter o JSON nos detalhes da reparação
            var reparacao = await response.Content
                .ReadFromJsonAsync<Reparacao>();

            return reparacao ?? throw new Exception(
                "Não foi possível obter os detalhes da reparação.");
        }
    }
}
