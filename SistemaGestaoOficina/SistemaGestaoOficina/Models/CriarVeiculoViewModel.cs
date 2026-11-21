using System.ComponentModel.DataAnnotations;

namespace SistemaGestaoOficina.Models
{
    public class CriarVeiculoViewModel
    {
        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public string Marca { get; set; } = string.Empty;

        [Required]
        public string Modelo { get; set; } = string.Empty;

        [Required]
        public string Matricula { get; set; } = string.Empty;

        [Required]
        public int Ano { get; set; }

        [Required]
        public int Quilometragem { get; set; }

        [Required]
        public string Combustivel { get; set; } = string.Empty;
    }
}
