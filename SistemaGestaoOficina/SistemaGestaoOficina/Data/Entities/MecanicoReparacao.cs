namespace SistemaGestaoOficina.Data.Entities
{
    public class MecanicoReparacao
    {
        public int Id { get; set; }

        public Mecanico Mecanico { get; set; }

        public Reparacao Reparacao { get; set; }
    }
}
