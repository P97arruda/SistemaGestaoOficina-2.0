using System.ComponentModel.DataAnnotations;

namespace SistemaGestaoOficina.Models
{
    public class ChangePasswordViewModel
    {
       
            [Required]
            [Display(Name = "Password atual")]
            public string OldPassword { get; set; }

            [Required]
            [Display(Name = "Nova password")]
            public string NewPassword { get; set; }

            [Required]
            [Compare("NewPassword")]
            [Display(Name = "Confirmar nova password")]
            public string Confirm { get; set; }
        
    }
}
