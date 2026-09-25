using System.ComponentModel.DataAnnotations;

namespace SistemaGestaoOficina.Data.Entities
{
    public class Cliente : IEntity
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(9, ErrorMessage = " O campo {0} pode conter no máximo {1} caracteres. ")]
        public string NIF { get; set; }

        public User User { get; set; }

        public ICollection<Veiculo> Veiculos { get; set; }

        public ICollection<Marcacao> Marcacaos { get; set; }
    }
}
