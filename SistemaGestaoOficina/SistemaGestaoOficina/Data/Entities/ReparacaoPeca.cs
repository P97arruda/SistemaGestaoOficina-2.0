using System.ComponentModel.DataAnnotations;

namespace SistemaGestaoOficina.Data.Entities
{
    public class ReparacaoPeca : IEntity
    {
        public int Id { get; set; }

        [Required]
        public int ReparacaoId { get; set; }

        public Reparacao? Reparacao { get; set; }

        [Required]
        public int PecaId { get; set; }

        public Peca? Peca { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "A quantidade deve ser superior a zero.")]
        public int Quantidade { get; set; }

        [Required]
        [Range(0.01, 999999.99, ErrorMessage = "O preço deve ser superior a zero.")]
        public decimal PrecoUnitario { get; set; }
    }
}
