
using Microsoft.Extensions.DependencyInjection;
using OficinaCET107.Mobile.Services;

namespace OficinaCET107.Mobile.Views
{
    public partial class LoginPage : ContentPage
    {
        public LoginPage()
        {
            InitializeComponent();
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            var email = EmailEntry.Text?.Trim();
            var password = PasswordEntry.Text;

            if (string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password))
            {
                await DisplayAlertAsync(
                    "Atenção",
                    "Preencha o email e a password.",
                    "OK");

                return;
            }

            LoginButton.IsEnabled = false;

            try
            {
                var authService = Handler!.MauiContext!
                    .Services.GetRequiredService<AuthService>();

                var sucesso = await authService.LoginAsync(
                    email,
                    password);

                if (sucesso)
                {
                    await Shell.Current.GoToAsync(nameof(DashboardPage));
                }
                else
                {
                    await DisplayAlertAsync(
                        "Erro",
                        "Email ou password inválidos.",
                        "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync(
                    "Erro de ligação",
                    ex.Message + "\n" + ex.InnerException?.Message,
                    "OK");
            }

            finally
            {
                LoginButton.IsEnabled = true;
            }
        }
    }
}
