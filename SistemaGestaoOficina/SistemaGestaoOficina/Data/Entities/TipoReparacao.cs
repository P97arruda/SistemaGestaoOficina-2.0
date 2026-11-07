using System.ComponentModel.DataAnnotations;

namespace SistemaGestaoOficina.Data.Entities
{
    public class TipoReparacao : IEntity
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(70)]
        public string Nome { get; set; }

        public ICollection<ReparacaoTipoReparacao>? ReparacaoTipoReparacoes { get; set; }
    }
}
