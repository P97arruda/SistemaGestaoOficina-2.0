using System.ComponentModel.DataAnnotations;

namespace SistemaGestaoOficina.Models
{
    public class ChangeUserViewModel
    {
        [Required]
        [Display(Name = "Nome")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Apelido")]
        public string LastName { get; set; }

        [MaxLength(20, ErrorMessage = "O campo {0} pode conter no máximo {1} caracteres.")]
        [Display(Name = "Contacto")]
        public string PhoneNumber { get; set; }
    }
}

