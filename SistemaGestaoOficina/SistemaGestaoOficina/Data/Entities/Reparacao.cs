using System.ComponentModel.DataAnnotations;

namespace SistemaGestaoOficina.Data.Entities
{
    public class Reparacao : IEntity
    {
        public int Id { get; set; }

        public decimal CustoTotal { get; set; }

        [Required]
        public DateTime DataInicio { get; set; }

        public DateTime? DataFim { get; set; }

        public bool Concluida { get; set; }

        [Required]
        [MaxLength(50)]
        public string TipoServico { get; set; }

        [Required]
        public int MarcacaoId { get; set; }

        public Marcacao? Marcacao { get; set; }

        public ICollection<MecanicoReparacao> MecanicoReparacaos { get; set; }

        public ICollection<ReparacaoTipoReparacao>? ReparacaoTipoReparacoes { get; set; }

        
        public ICollection<ReparacaoPeca>? ReparacaoPecas { get; set; }
    }
}
