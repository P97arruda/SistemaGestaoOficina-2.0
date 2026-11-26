
using OficinaCET107.Mobile.Services;

namespace OficinaCET107.Mobile.Views
{
    public partial class DashboardPage : ContentPage
    {
        private bool _abrindoVeiculos;
        private bool _abrindoMarcacoes;
        private bool _abrindoReparacoes;
        private bool _abrindoPerfil;

        private readonly AuthService _authService;

        private bool _terminandoSessao;

        public DashboardPage(AuthService authService)
        {
            InitializeComponent();
            _authService = authService;
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

        // HISTÓRICO DE REPARAÇÕES
        private async void OnReparacoesTapped(object sender, TappedEventArgs e)
        {
            if (_abrindoReparacoes)
                return;

            _abrindoReparacoes = true;

            try
            {
                await ReparacoesCard.ScaleTo(0.97, 80);
                await ReparacoesCard.ScaleTo(1, 80);

                await Shell.Current.GoToAsync(
                    nameof(ReparacoesPage));
            }
            finally
            {
                _abrindoReparacoes = false;
            }
        }

        // MEU PERFIL
        private async void OnPerfilTapped(object sender, TappedEventArgs e)
        {
            if (_abrindoPerfil)
                return;

            _abrindoPerfil = true;

            try
            {
                await PerfilCard.ScaleTo(0.97, 80);
                await PerfilCard.ScaleTo(1, 80);

                await Shell.Current.GoToAsync(
                    nameof(PerfilPage));
            }
            finally
            {
                _abrindoPerfil = false;
            }
        }

        // TERMINAR SESSÃO
        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            if (_terminandoSessao)
                return;

            _terminandoSessao = true;
            LogoutButton.IsEnabled = false;

            try
            {
                // Remover o token
                _authService.Logout();

                // Voltar ao Login
                await Shell.Current.GoToAsync("//LoginPage");
            }
            finally
            {
                _terminandoSessao = false;
                LogoutButton.IsEnabled = true;
            }
        }


    }
}
