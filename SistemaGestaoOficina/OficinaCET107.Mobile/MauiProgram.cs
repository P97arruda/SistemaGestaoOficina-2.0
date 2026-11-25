
using Microsoft.Extensions.Logging;
using OficinaCET107.Mobile.Services;
using OficinaCET107.Mobile.Views;

namespace OficinaCET107.Mobile
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Configuração da API local
            var handler = new HttpClientHandler();

#if DEBUG
            handler.ServerCertificateCustomValidationCallback =
                (request, cert, chain, errors) =>
                    errors == System.Net.Security.SslPolicyErrors.None ||
                    (request.RequestUri?.Host == "10.0.2.2" &&
                     request.RequestUri.Port == 7204 &&
                     cert?.Subject.Contains("CN=localhost") == true);
#endif

            builder.Services.AddSingleton(new HttpClient(handler)
            {
                BaseAddress = new Uri("https://10.0.2.2:7204/")
            });

            // Services
            builder.Services.AddTransient<AuthService>();
            builder.Services.AddTransient<VeiculoService>();
            builder.Services.AddTransient<VeiculosPage>();
            builder.Services.AddTransient<DetalhesVeiculoPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
