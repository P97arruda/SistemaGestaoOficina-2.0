using System.ComponentModel.DataAnnotations;

namespace SistemaGestaoOficina.Models
{
    public class ChangePasswordViewModel
    {
        
            [Required]
            [DataType(DataType.Password)]
            [Display(Name = "Password atual")]
            public string OldPassword { get; set; } = string.Empty;

            [Required]
            [MinLength(8, ErrorMessage = "A password deve ter pelo menos 8 caracteres.")]
            [DataType(DataType.Password)]
            [Display(Name = "Nova password")]
            public string NewPassword { get; set; } = string.Empty;

            [Required]
            [Compare("NewPassword", ErrorMessage = "As passwords não coincidem.")]
            [DataType(DataType.Password)]
            [Display(Name = "Confirmar nova password")]
            public string Confirm { get; set; } = string.Empty;
        

    }
}
