using System.ComponentModel.DataAnnotations;

namespace SistemaGestaoOficina.Models
{
    public class ReparacaoViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Selecione uma marcação.")]
        public int? MarcacaoId { get; set; }

        [Required(ErrorMessage = "Selecione o tipo de serviço.")]
        [MaxLength(50)]
        public string TipoServico { get; set; } = string.Empty;

        [Required(ErrorMessage = "Introduza a data de início.")]
        public DateTime? DataInicio { get; set; }

        public DateTime? DataFim { get; set; }

        public bool Concluida { get; set; }

        [Range(0, 999999.99, ErrorMessage = "O custo não pode ser negativo.")]
        public decimal CustoTotal { get; set; }

        public List<int> MecanicoIds { get; set; } = new();

        public List<int> PecaIds { get; set; } = new();
    }
}

