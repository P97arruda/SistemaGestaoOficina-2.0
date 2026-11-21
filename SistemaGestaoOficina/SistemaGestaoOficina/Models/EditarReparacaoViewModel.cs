using System.ComponentModel.DataAnnotations;

namespace SistemaGestaoOficina.Models
{
    public class EditarReparacaoViewModel
    {
        public int Id { get; set; }

        // Dados apenas para visualização
        public string NomeCliente { get; set; } = string.Empty;

        public string Veiculo { get; set; } = string.Empty;

        public string Matricula { get; set; } = string.Empty;

        public DateTime DataInicio { get; set; }

        public bool Concluida { get; set; }

        public string NomeMecanicoAtual { get; set; } = string.Empty;

        // Tipo de reparação selecionado
        [Required(ErrorMessage = "Selecione o tipo de reparação.")]
        public int? TipoReparacaoId { get; set; }

        // Mecânico substituto (opcional)
        public int? MecanicoId { get; set; }
    }
}
