using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace SistemaGestaoOficina.Data.Entities
{
    public class User : IdentityUser
    {
        [Required]
        [MaxLength(50)]
        [Display(Name = "Nome")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        [Display(Name = "Apelido")]
        public string LastName { get; set; } = string.Empty;

        [MaxLength(9, ErrorMessage = "O campo {0} pode conter no máximo {1} caracteres.")]
        [Display(Name = "NIF")]
        public string? NIF { get; set; }

        [Display(Name = "Nome")]
        public string FullName => $"{FirstName} {LastName}";
    }
}
