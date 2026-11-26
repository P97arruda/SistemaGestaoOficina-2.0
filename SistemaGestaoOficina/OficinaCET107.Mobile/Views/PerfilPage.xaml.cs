
using OficinaCET107.Mobile.Services;

namespace OficinaCET107.Mobile.Views
{
    public partial class PerfilPage : ContentPage
    {
        private readonly PerfilService _perfilService;
        private readonly HttpClient _httpClient;

        public PerfilPage(
            PerfilService perfilService,
            HttpClient httpClient)
        {
            InitializeComponent();

            _perfilService = perfilService;
            _httpClient = httpClient;
        }

        // CARREGAR DADOS DO PERFIL
        protected override async void OnAppearing()
        {
            base.OnAppearing();

            LoadingIndicator.IsRunning = true;
            LoadingIndicator.IsVisible = true;
            PerfilContent.IsVisible = false;

            try
            {
                // CONSULTAR O PERFIL NA API
                var perfil = await _perfilService
                    .GetMeuPerfilAsync();

                // NOME COMPLETO
                var nomeCompleto =
                    $"{perfil.FirstName} {perfil.LastName}".Trim();

                NomeCompletoLabel.Text = nomeCompleto;
                NomeLabel.Text = nomeCompleto;

                // NIF
                NifLabel.Text = string.IsNullOrWhiteSpace(perfil.NIF)
                    ? "Não informado"
                    : perfil.NIF;

                // EMAIL
                EmailLabel.Text = string.IsNullOrWhiteSpace(perfil.Email)
                    ? "Não informado"
                    : perfil.Email;

                // TELEFONE
                TelefoneLabel.Text =
                    string.IsNullOrWhiteSpace(perfil.PhoneNumber)
                    ? "Não informado"
                    : perfil.PhoneNumber;

                // MOSTRAR PERFIL
                PerfilContent.IsVisible = true;

                // CARREGAR FOTO
                await CarregarFotoAsync(perfil.FotoUrl);
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync(
                    "Erro",
                    "Não foi possível carregar o perfil.\n"
                    + ex.Message,
                    "OK");
            }
            finally
            {
                LoadingIndicator.IsRunning = false;
                LoadingIndicator.IsVisible = false;
            }
        }

        // CARREGAR FOTO DO PERFIL
        private async Task CarregarFotoAsync(string? fotoUrl)
        {
            // MOSTRAR AVATAR PADRÃO
            AvatarPadrao.IsVisible = true;
            FotoPerfilImage.IsVisible = false;
            FotoPerfilImage.Source = null;

            if (string.IsNullOrWhiteSpace(fotoUrl))
                return;

            try
            {
                Uri endereco;

                if (Uri.TryCreate(
                    fotoUrl,
                    UriKind.Absolute,
                    out var url))
                {
                    var builder = new UriBuilder(url);

#if ANDROID
                    // ACESSO AO SERVIDOR LOCAL PELO EMULADOR
                    if (builder.Host == "localhost" ||
                        builder.Host == "127.0.0.1")
                    {
                        builder.Host = "10.0.2.2";
                    }
#endif

                    endereco = builder.Uri;
                }
                else
                {
                    endereco = new Uri(
                        _httpClient.BaseAddress!,
                        fotoUrl);
                }

                // CARREGAR IMAGEM UTILIZANDO O HTTPCLIENT
                var imagem = await _httpClient
                    .GetByteArrayAsync(endereco);

                FotoPerfilImage.Source =
                    ImageSource.FromStream(
                        () => new MemoryStream(imagem));

                // MOSTRAR FOTO DO CLIENTE
                AvatarPadrao.IsVisible = false;
                FotoPerfilImage.IsVisible = true;
            }
            catch
            {
                // SE A FOTO FALHAR, MANTER AVATAR PADRÃO
                AvatarPadrao.IsVisible = true;
                FotoPerfilImage.IsVisible = false;
            }
        }
    }
}
