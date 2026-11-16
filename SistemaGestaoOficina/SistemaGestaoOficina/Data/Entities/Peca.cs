using System.ComponentModel.DataAnnotations;
using System.Drawing;

namespace SistemaGestaoOficina.Data.Entities
{
    public class Peca :IEntity
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome da peça é obrigatório.")]
        [MaxLength(100)]
        [Display(Name = "Nome da Peça")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "O preço é obrigatório.")]
        [Range(0.01, 999999.99, ErrorMessage = "O preço deve ser superior a zero.")]
        [Display(Name = "Preço (€)")]
        public decimal Preco { get; set; }

        [Display(Name = "Ativa")]
        public bool Ativa { get; set; } = true;

        public ICollection<ReparacaoPeca>? ReparacaoPecas { get; set; }
    }
}
