namespace SistemaGestaoOficina.Data.Entities
{
    public class ReparacaoTipoReparacao
    {
        public int Id { get; set; }

        public int ReparacaoId { get; set; }

        public Reparacao Reparacao { get; set; }

        public int TipoReparacaoId { get; set; }

        public TipoReparacao TipoReparacao { get; set; }
    }
}
