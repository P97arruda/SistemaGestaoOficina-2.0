using System.ComponentModel.DataAnnotations;

namespace SistemaGestaoOficina.Models
{
    public class CreateUserViewModel
    {
        [Required]
        [Display(Name = "Nome")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Apelido")]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [Display(Name = "Contacto")]
        public string PhoneNumber { get; set; }

        [Required]
        [Display(Name = "Tipo de Utilizador")]
        public string Role { get; set; }
    }
}
