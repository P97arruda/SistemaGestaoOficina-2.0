using System.ComponentModel.DataAnnotations;

namespace SistemaGestaoOficina.Models
{
    public class EnviarComunicadoViewModel
    {
        [Required(ErrorMessage = "Introduza o assunto.")]
        [StringLength(120)]
        public string Assunto { get; set; } = string.Empty;

        [Required(ErrorMessage = "Introduza a mensagem.")]
        [StringLength(3000)]
        public string Mensagem { get; set; } = string.Empty;

        public List<string> DestinatariosIds { get; set; } = new();
    }
}
