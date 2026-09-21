namespace SistemaGestaoOficina.Data.Entities
{
    public class Marcacao
    {
        public int Id { get; set; }

        public string TipoServico { get; set; }

        public DateTime DataHora { get; set; }

        public string Estado { get; set; }
    }
}
