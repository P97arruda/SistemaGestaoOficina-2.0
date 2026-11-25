
using OficinaCET107.Mobile.Services;

namespace OficinaCET107.Mobile.Views
{
    public partial class DetalhesVeiculoPage : ContentPage, IQueryAttributable
    {
        private readonly VeiculoService _veiculoService;
        private int _veiculoId;

        public DetalhesVeiculoPage(VeiculoService veiculoService)
        {
            InitializeComponent();
            _veiculoService = veiculoService;
        }

        // Receber o ID do veículo selecionado
        public void ApplyQueryAttributes(
            IDictionary<string, object> query)
        {
            if (query.TryGetValue("id", out var valor) &&
                int.TryParse(valor?.ToString(), out var id))
            {
                _veiculoId = id;
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            LoadingIndicator.IsRunning = true;
            LoadingIndicator.IsVisible = true;
            DetalhesContent.IsVisible = false;

            try
            {
                // Consultar os detalhes na API
                var veiculo = await _veiculoService
                    .GetDetalhesVeiculoAsync(_veiculoId);

                if (veiculo == null)
                {
                    await DisplayAlertAsync(
                        "Atenção",
                        "Veículo não encontrado.",
                        "OK");

                    return;
                }
                // Apresentar os dados do veículo
                MarcaLabel.Text = veiculo.Marca;
                ModeloLabel.Text = veiculo.Modelo;
                AnoLabel.Text = veiculo.Ano.ToString();
                MatriculaLabel.Text = veiculo.Matricula;

                QuilometragemLabel.Text =
                    $"{veiculo.Quilometragem:N0} km";

                CombustivelLabel.Text =
                    string.IsNullOrWhiteSpace(veiculo.Combustivel)
                        ? "Não informado"
                        : veiculo.Combustivel;

                DetalhesContent.IsVisible = true;
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync(
                    "Erro",
                    "Não foi possível carregar o veículo.\n"
                    + ex.Message,
                    "OK");
            }
            finally
            {
                LoadingIndicator.IsRunning = false;
                LoadingIndicator.IsVisible = false;
            }
        }
    }
}
