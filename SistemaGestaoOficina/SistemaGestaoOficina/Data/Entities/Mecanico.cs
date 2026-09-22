using System.ComponentModel.DataAnnotations;

namespace SistemaGestaoOficina.Data.Entities
{
    public class Mecanico
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Nome { get; set; }

        [Required]
        [MaxLength(50)]
        public string Especialidade { get; set; }

        [Required]
        [MaxLength(20)]
        public string Horario { get; set; }

        [Required]
        [MaxLength(15)]
        public string Contacto { get; set; }

        public bool Ativo { get; set; }

        public ICollection<Marcacao> Marcacaos { get; set; }

        public ICollection<MecanicoReparacao> MecanicoReparacaoes { get; set; }
    }
}
