
namespace OficinaCET107.Mobile.Views
{
    public partial class DashboardPage : ContentPage
    {
        private bool _abrindoVeiculos;

        public DashboardPage()
        {
            InitializeComponent();
        }

        private async void OnVeiculosTapped(
            object sender, TappedEventArgs e)
        {
            if (_abrindoVeiculos)
                return;

            _abrindoVeiculos = true;

            try
            {
                // Pequena animação ao clicar
                await VeiculosCard.ScaleTo(0.97, 80);
                await VeiculosCard.ScaleTo(1, 80);

                // Abrir a página de veículos
                await Shell.Current.GoToAsync(
                    nameof(VeiculosPage));
            }
            finally
            {
                _abrindoVeiculos = false;
            }
        }
    }
}
