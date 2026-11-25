
using OficinaCET107.Mobile.Services;

namespace OficinaCET107.Mobile.Views
{
    public partial class VeiculosPage : ContentPage
    {
        private readonly VeiculoService _veiculoService;
        private bool _abrindoDetalhes;

        public VeiculosPage(VeiculoService veiculoService)
        {
            InitializeComponent();
            _veiculoService = veiculoService;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            LoadingIndicator.IsRunning = true;
            LoadingIndicator.IsVisible = true;
            VeiculosCollection.IsVisible = false;

            try
            {
                var veiculos = await _veiculoService
                    .GetMeusVeiculosAsync();

                VeiculosCollection.ItemsSource = veiculos;
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync(
                    "Erro",
                    "Não foi possível carregar os veículos.\n"
                    + ex.Message,
                    "OK");
            }
            finally
            {
                LoadingIndicator.IsRunning = false;
                LoadingIndicator.IsVisible = false;
                VeiculosCollection.IsVisible = true;
            }
        }

        private async void OnVeiculoTapped(
            object sender, TappedEventArgs e)
        {
            if (_abrindoDetalhes)
                return;

            if (e.Parameter is not int veiculoId)
                return;

            _abrindoDetalhes = true;

            try
            {
                // Animação do cartão selecionado
                if (sender is TapGestureRecognizer gesture &&
                    gesture.Parent is Border card)
                {
                    await card.ScaleTo(0.97, 70);
                    await card.ScaleTo(1, 70);
                }

                // Navegar enviando o ID do veículo
                await Shell.Current.GoToAsync(
                    nameof(DetalhesVeiculoPage),
                    new Dictionary<string, object>
                    {
                        { "id", veiculoId }
                    });
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync(
                    "Erro",
                    "Não foi possível abrir o veículo.\n"
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
