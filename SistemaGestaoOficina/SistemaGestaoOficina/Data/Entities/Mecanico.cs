namespace SistemaGestaoOficina.Data.Entities
{
    public class Mecanico
    {
        public int Id { get; set; }

        public string Nome { get; set; }

        public string Especialidade { get; set; }

        public string Horario { get; set; }

        public string Contacto { get; set; }

        public bool Ativo { get; set; }
    }
}
