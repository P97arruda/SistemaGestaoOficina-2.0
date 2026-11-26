
namespace OficinaCET107.Mobile.Views
{
    public partial class DashboardPage : ContentPage
    {
        private bool _abrindoVeiculos;
        private bool _abrindoMarcacoes;

        public DashboardPage()
        {
            InitializeComponent();
        }

        // MEUS VEÍCULOS
        private async void OnVeiculosTapped(object sender, TappedEventArgs e)
        {
            if (_abrindoVeiculos)
                return;

            _abrindoVeiculos = true;

            try
            {
                await VeiculosCard.ScaleTo(0.97, 80);
                await VeiculosCard.ScaleTo(1, 80);

                await Shell.Current.GoToAsync(
                    nameof(VeiculosPage));
            }
            finally
            {
                _abrindoVeiculos = false;
            }
        }

        // MINHAS MARCAÇÕES
        private async void OnMarcacoesTapped(object sender, TappedEventArgs e)
        {
            if (_abrindoMarcacoes)
                return;

            _abrindoMarcacoes = true;

            try
            {
                await MarcacoesCard.ScaleTo(0.97, 80);
                await MarcacoesCard.ScaleTo(1, 80);

                await Shell.Current.GoToAsync(
                    nameof(MarcacoesPage));
            }
            finally
            {
                _abrindoMarcacoes = false;
            }
        }
    }
}

