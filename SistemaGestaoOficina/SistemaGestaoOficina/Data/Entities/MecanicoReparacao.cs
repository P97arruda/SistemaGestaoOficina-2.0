namespace SistemaGestaoOficina.Data.Entities
{
    public class MecanicoReparacao
    {
        public int Id { get; set; }

        public int MecanicoId { get; set; }
        public Mecanico? Mecanico { get; set; }

        public int ReparacaoId { get; set; }
        public Reparacao? Reparacao { get; set; }
    }
}
