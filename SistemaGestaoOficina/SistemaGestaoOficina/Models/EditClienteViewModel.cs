using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace SistemaGestaoOficina.Models
{
    public class EditClienteViewModel
    {
        public string Id { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Nome")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Apelido")]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Contacto")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Display(Name = "NIF")]
        [MaxLength(9, ErrorMessage = "O campo {0} pode conter no máximo {1} caracteres.")]
        public string? NIF { get; set; }

        // Nova fotografia selecionada pelo rececionista
        [Display(Name = "Fotografia")]
        public IFormFile? ImageFile { get; set; }

        // URL da fotografia atual do cliente
        public string? ImageUrl { get; set; }
    }
}
