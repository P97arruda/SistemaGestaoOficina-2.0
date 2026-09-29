using System.ComponentModel.DataAnnotations;

namespace SistemaGestaoOficina.Models
{
    public class RecoverPasswordViewModel
    {
        
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        
    }
}
