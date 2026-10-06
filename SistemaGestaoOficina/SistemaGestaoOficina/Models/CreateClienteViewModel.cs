using System.ComponentModel.DataAnnotations;

namespace SistemaGestaoOficina.Models
{
    public class CreateClienteViewModel
    {
        [Required]
        [Display(Name = "Nome")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Apelido")]
        public string LastName { get; set; } = string.Empty;

        [Display(Name = "NIF")]
        [MaxLength(9, ErrorMessage = "O campo {0} pode conter no máximo {1} caracteres.")]
        public string? NIF { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Contacto")]
        public string PhoneNumber { get; set; } = string.Empty;
    }
}
