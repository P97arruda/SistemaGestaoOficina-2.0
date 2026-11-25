using System.Net;
using System.Net.Http.Json;
using Microsoft.Maui.Storage;
using OficinaCET107.Mobile.Models;

namespace OficinaCET107.Mobile.Services
{
    public class AuthService
    {
        private readonly HttpClient _httpClient;

        public AuthService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<bool> LoginAsync(string email,string password)
        {
            // Enviar email e password para a API
            var response = await _httpClient.PostAsJsonAsync(
                "api/mobile/login",
                new
                {
                    Email = email.Trim(),
                    Password = password
                });

            // Credenciais inválidas
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return false;
            }

            // Verificar outros erros HTTP
            response.EnsureSuccessStatusCode();

            // Ler o token devolvido pela API
            var resultado = await response.Content.ReadFromJsonAsync<MobileLoginResponse>();

            if (resultado == null || string.IsNullOrWhiteSpace(resultado.AccessToken))
            {
                return false;
            }

            // Guardar o token no dispositivo
            await SecureStorage.Default.SetAsync("access_token", resultado.AccessToken);

            return true;
        }

        public async Task<string?> GetTokenAsync()
        {
            return await SecureStorage.Default.GetAsync("access_token");
        }

        public void Logout()
        {
            SecureStorage.Default.Remove("access_token");
        }
    }
}
