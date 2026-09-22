using System.ComponentModel.DataAnnotations;

namespace SistemaGestaoOficina.Data.Entities
{
    public class Reparacao
    {
        public int Id { get; set; }

        [MaxLength(50)]
        public string Pecas { get; set; }

        public decimal CustoTotal { get; set; }

        [Required]
        public DateTime DataInicio { get; set; }

        public DateTime? DataFim { get; set; }

        public bool Concluida { get; set; }

        [Required]
        [MaxLength(50)]
        public string TipoServico { get; set; }

        public Marcacao Marcacao { get; set; }

        public ICollection<MecanicoReparacao> MecanicoReparacaos { get; set; }
    }
}
