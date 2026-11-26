
using System.Net.Http.Headers;
using System.Net.Http.Json;
using OficinaCET107.Mobile.Models;

namespace OficinaCET107.Mobile.Services
{
    public class MarcacaoService
    {
        private readonly HttpClient _httpClient;
        private readonly AuthService _authService;

        public MarcacaoService(
            HttpClient httpClient,
            AuthService authService)
        {
            _httpClient = httpClient;
            _authService = authService;
        }

        public async Task<List<Marcacao>> GetMinhasMarcacoesAsync()
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
                "api/mobile/cliente/marcacoes");

            // Enviar o token Bearer
            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            // Consultar a API
            using var response =
                await _httpClient.SendAsync(request);

            response.EnsureSuccessStatusCode();

            // Converter o JSON numa lista de marcações
            var marcacoes = await response.Content
                .ReadFromJsonAsync<List<Marcacao>>();

            return marcacoes ?? new List<Marcacao>();
        }
    }
}
