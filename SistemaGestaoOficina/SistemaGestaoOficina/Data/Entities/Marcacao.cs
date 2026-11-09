using System.ComponentModel.DataAnnotations;

namespace SistemaGestaoOficina.Data.Entities
{
    public class Marcacao : IEntity
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

        public string UserId { get; set; }

        public User User { get; set; }

        [Required]
        public int VeiculoId { get; set; }

        public Veiculo Veiculo { get; set; }

        [Required]
        public int MecanicoId { get; set; }
        public Mecanico Mecanico { get; set; }
    }
}
