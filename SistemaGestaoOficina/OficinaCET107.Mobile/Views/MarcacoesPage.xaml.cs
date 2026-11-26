
using OficinaCET107.Mobile.Models;
using OficinaCET107.Mobile.Services;

namespace OficinaCET107.Mobile.Views
{
    public partial class MarcacoesPage : ContentPage
    {
        private readonly MarcacaoService _marcacaoService;
        private bool _abrindoDetalhes;

        public MarcacoesPage(MarcacaoService marcacaoService)
        {
            InitializeComponent();
            _marcacaoService = marcacaoService;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            LoadingIndicator.IsRunning = true;
            LoadingIndicator.IsVisible = true;
            MarcacoesCollection.IsVisible = false;

            try
            {
                var marcacoes = await _marcacaoService
                    .GetMinhasMarcacoesAsync();

                MarcacoesCollection.ItemsSource = marcacoes;
                MarcacoesCollection.IsVisible = true;
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync(
                    "Erro",
                    "Não foi possível carregar as marcações.\n"
                    + ex.Message,
                    "OK");
            }
            finally
            {
                LoadingIndicator.IsRunning = false;
                LoadingIndicator.IsVisible = false;
            }
        }

        // ABRIR DETALHES DA MARCAÇÃO
        private async void OnMarcacaoTapped(
            object sender,
            TappedEventArgs e)
        {
            if (_abrindoDetalhes)
                return;

            if (e.Parameter is not Marcacao marcacao)
                return;

            try
            {
                _abrindoDetalhes = true;

                await Shell.Current.GoToAsync(
                    nameof(DetalhesMarcacaoPage),
                    new Dictionary<string, object>
                    {
                        { "marcacao", marcacao }
                    });
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync(
                    "Erro",
                    "Não foi possível abrir os detalhes.\n"
                    + ex.Message,
                    "OK");
            }
            finally
            {
                _abrindoDetalhes = false;
            }
        }
    }
}
