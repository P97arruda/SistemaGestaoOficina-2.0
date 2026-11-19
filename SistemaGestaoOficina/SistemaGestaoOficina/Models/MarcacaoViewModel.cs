using System.ComponentModel.DataAnnotations;

namespace SistemaGestaoOficina.Models
{
    public class MarcacaoViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Selecione um cliente.")]
        public string UserId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Selecione um veículo.")]
        public int? VeiculoId { get; set; }

        [Required(ErrorMessage = "Selecione um mecânico.")]
        public int? MecanicoId { get; set; }

        [Required(ErrorMessage = "Selecione o tipo de serviço.")]
        [MaxLength(50)]
        public string TipoServico { get; set; } = string.Empty;

        [Required(ErrorMessage = "Selecione a data e hora.")]
        public DateTime? DataHora { get; set; }
    }
}
