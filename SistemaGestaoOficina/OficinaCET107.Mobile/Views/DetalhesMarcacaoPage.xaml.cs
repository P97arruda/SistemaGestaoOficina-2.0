
using OficinaCET107.Mobile.Models;

namespace OficinaCET107.Mobile.Views
{
    public partial class DetalhesMarcacaoPage : ContentPage, IQueryAttributable
    {
        public DetalhesMarcacaoPage()
        {
            InitializeComponent();
        }

        // Receber a marcação selecionada
        public void ApplyQueryAttributes(
            IDictionary<string, object> query)
        {
            if (!query.TryGetValue("marcacao", out var valor) ||
                valor is not Marcacao marcacao)
            {
                return;
            }

            // Apresentar os dados
            TipoServicoLabel.Text = marcacao.TipoServico;

            VeiculoLabel.Text = marcacao.Veiculo;

            MatriculaLabel.Text = marcacao.Matricula;

            DataHoraLabel.Text = marcacao.DataHora.ToString("dd/MM/yyyy HH:mm");

            EstadoLabel.Text = marcacao.Estado;

            MecanicoLabel.Text = string.IsNullOrWhiteSpace(marcacao.Mecanico)
                    ? "A definir"
                    : marcacao.Mecanico;

            DetalhesContent.IsVisible = true;
        }
    }
}
