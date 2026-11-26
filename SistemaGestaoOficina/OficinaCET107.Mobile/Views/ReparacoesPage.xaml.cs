
using OficinaCET107.Mobile.Models;
using OficinaCET107.Mobile.Services;

namespace OficinaCET107.Mobile.Views
{
    public partial class ReparacoesPage : ContentPage
    {
        private readonly ReparacaoService _reparacaoService;
        private bool _abrindoDetalhes;

        public ReparacoesPage(ReparacaoService reparacaoService)
        {
            InitializeComponent();
            _reparacaoService = reparacaoService;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            LoadingIndicator.IsRunning = true;
            LoadingIndicator.IsVisible = true;
            ReparacoesCollection.IsVisible = false;

            try
            {
                var reparacoes = await _reparacaoService
                    .GetHistoricoReparacoesAsync();

                ReparacoesCollection.ItemsSource = reparacoes;
                ReparacoesCollection.IsVisible = true;
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync(
                    "Erro",
                    "Não foi possível carregar as reparações.\n"
                    + ex.Message,
                    "OK");
            }
            finally
            {
                LoadingIndicator.IsRunning = false;
                LoadingIndicator.IsVisible = false;
            }
        }

        // ABRIR DETALHES DA REPARAÇÃO
        private async void OnReparacaoTapped(
            object sender,
            TappedEventArgs e)
        {
            if (_abrindoDetalhes)
                return;

            if (e.Parameter is not Reparacao reparacao)
                return;

            _abrindoDetalhes = true;

            try
            {
                await Shell.Current.GoToAsync(
                    nameof(DetalhesReparacaoPage),
                    new Dictionary<string, object>
                    {
                        { "reparacao", reparacao }
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
