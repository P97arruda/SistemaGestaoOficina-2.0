using System.ComponentModel.DataAnnotations;

namespace SistemaGestaoOficina.Data.Entities
{
    public class Marcacao
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string TipoServico { get; set; }

        [Required]
        public DateTime DataHora { get; set; }

        [Required]
        [MaxLength(30)]
        public string Estado { get; set; }

        public Cliente Cliente { get; set; }

        public Veiculo Veiculo { get; set; }

        public Mecanico Mecanico { get; set; }
    }
}
