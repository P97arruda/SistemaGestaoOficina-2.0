using System.ComponentModel.DataAnnotations;

namespace SistemaGestaoOficina.Data.Entities
{
    public class Veiculo
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(15)]
        public string Marca { get; set; }


        [Required]
        [MaxLength(20)]
        public string Modelo { get; set; }

        [Required]
        [MaxLength(10)]
        public string Matricula { get; set; }

        public int Ano { get; set; }

        public int Quilometragem { get; set; }

        [Required]
        [MaxLength(30)]
        public string Combustivel { get; set; }

        public Cliente Cliente { get; set; }

        public ICollection<Marcacao> Marcacaos { get; set;}
    }
}
