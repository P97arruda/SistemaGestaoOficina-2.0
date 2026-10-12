using System.ComponentModel.DataAnnotations;

namespace SistemaGestaoOficina.Models
{
    public class VeiculoViewModel
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public string Marca { get; set; }

        [Required]
        public string Modelo { get; set; }

        [Required]
        public string Matricula { get; set; }

        [Required]
        public int Ano { get; set; }

        [Required]
        public int Quilometragem { get; set; }

        [Required]
        public string Combustivel { get; set; }
    }
}
