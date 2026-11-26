
using System.Net.Http.Headers;
using System.Net.Http.Json;
using OficinaCET107.Mobile.Models;

namespace OficinaCET107.Mobile.Services
{
    public class PerfilService
    {
        private readonly HttpClient _httpClient;
        private readonly AuthService _authService;

        public PerfilService(
            HttpClient httpClient,
            AuthService authService)
        {
            _httpClient = httpClient;
            _authService = authService;
        }

        // CONSULTAR PERFIL DO CLIENTE
        public async Task<Perfil> GetMeuPerfilAsync()
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
                "api/mobile/cliente/perfil");

            // Enviar o token Bearer
            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            // Consultar a API
            using var response =
                await _httpClient.SendAsync(request);

            response.EnsureSuccessStatusCode();

            // Converter o JSON nos dados do perfil
            var perfil = await response.Content
                .ReadFromJsonAsync<Perfil>();

            return perfil ?? throw new Exception(
                "Não foi possível obter os dados do perfil.");
        }
    }
}
