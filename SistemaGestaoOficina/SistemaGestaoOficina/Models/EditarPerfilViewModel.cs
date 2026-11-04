using System.ComponentModel.DataAnnotations;

namespace SistemaGestaoOficina.Models
{
    public class EditarPerfilViewModel
    {
        [Required]
        [MaxLength(50)]
        [Display(Name = "Nome")]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(50)]
        [Display(Name = "Apelido")]
        public string LastName { get; set; }

        [MaxLength(9)]
        [Display(Name = "NIF")]
        public string? NIF { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Phone]
        [Display(Name = "Telefone")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Foto de Perfil")]
        public IFormFile? ImageFile { get; set; }

        public string? ImageFullPath { get; set; }
    }
}
