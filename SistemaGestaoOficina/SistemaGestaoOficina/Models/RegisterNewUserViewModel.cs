using System.ComponentModel.DataAnnotations;

namespace SistemaGestaoOficina.Models
{
    public class RegisterNewUserViewModel
    {
        [Required]
        [Display(Name = "Nome")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Apelido")]
        public string LastName { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email")]
        public string Username { get; set; }

        [MaxLength(20, ErrorMessage = "O campo {0} pode conter no máximo {1} caracteres.")]
        [Display(Name = "Contacto")]
        public string PhoneNumber { get; set; }

        [Required]
        [MinLength(6)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [Compare("Password")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirmar password")]
        public string Confirm { get; set; }
    }
}

