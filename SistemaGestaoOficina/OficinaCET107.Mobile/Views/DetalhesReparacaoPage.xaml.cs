
using System.Globalization;
using OficinaCET107.Mobile.Models;
using OficinaCET107.Mobile.Services;

namespace OficinaCET107.Mobile.Views
{
    public partial class DetalhesReparacaoPage :
        ContentPage, IQueryAttributable
    {
        private readonly ReparacaoService _reparacaoService;
        private int _reparacaoId;

        public DetalhesReparacaoPage(
            ReparacaoService reparacaoService)
        {
            InitializeComponent();
            _reparacaoService = reparacaoService;
        }

        // RECEBER A REPARAÇÃO SELECIONADA
        public void ApplyQueryAttributes(
            IDictionary<string, object> query)
        {
            if (query.TryGetValue("reparacao", out var valor) &&
                valor is Reparacao reparacao)
            {
                _reparacaoId = reparacao.Id;
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (_reparacaoId <= 0)
                return;

            await CarregarDetalhesAsync();
        }

        // CARREGAR DETALHES DA REPARAÇÃO
        private async Task CarregarDetalhesAsync()
        {
            LoadingIndicator.IsRunning = true;
            LoadingIndicator.IsVisible = true;
            DetalhesContent.IsVisible = false;

            try
            {
                // CONSULTAR OS DETALHES NA API
                var reparacao = await _reparacaoService.GetDetalhesReparacaoAsync(_reparacaoId);

                var cultura = new CultureInfo("pt-PT");

                // IDENTIFICAÇÃO
                TipoServicoLabel.Text = reparacao.TipoServico;

                NumeroReparacaoLabel.Text =
                    $"Reparação #{reparacao.Id}";

                // VEÍCULO
                VeiculoLabel.Text = reparacao.Veiculo;
                MatriculaLabel.Text = reparacao.Matricula;

                // DATAS
                DataInicioLabel.Text =
                    reparacao.DataInicio.ToString(
                        "dd/MM/yyyy 'às' HH:mm", cultura);

                DataFimLabel.Text = reparacao.DataFim.HasValue
                    ? reparacao.DataFim.Value.ToString(
                        "dd/MM/yyyy 'às' HH:mm", cultura)
                    : "Não informada";

                // MECÂNICOS RESPONSÁVEIS
                var mecanicos = reparacao.Mecanicos
                    ?? new List<string>();

                MecanicosLabel.Text = mecanicos.Count > 0
                    ? string.Join(", ", mecanicos)
                    : "Não informado";

                // PEÇAS UTILIZADAS
                var pecas = reparacao.Pecas
                    ?? new List<ReparacaoPeca>();

                BindableLayout.SetItemsSource(
                    PecasLayout,
                    pecas);

                // MENSAGEM QUANDO NÃO EXISTEM PEÇAS
                SemPecasLabel.IsVisible =
                    pecas.Count == 0;

                // TOTAL DAS PEÇAS
                decimal totalPecas = pecas.Sum(
                    p => p.Subtotal);

                TotalPecasLabel.Text =
                    totalPecas.ToString("N2", cultura) + " €";

                // MÃO DE OBRA
                CustoMaoDeObraLabel.Text =
                    reparacao.CustoMaoDeObra.ToString(
                        "N2", cultura) + " €";

                // VALOR FINAL
                CustoTotalLabel.Text =
                    reparacao.CustoTotal.ToString(
                        "N2", cultura) + " €";

                // MOSTRAR CONTEÚDO
                DetalhesContent.IsVisible = true;
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync(
                    "Erro",
                    "Não foi possível carregar os detalhes.\n"
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
