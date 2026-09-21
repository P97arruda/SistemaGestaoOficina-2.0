namespace SistemaGestaoOficina.Data.Entities
{
    public class Reparacao
    {
        public int Id { get; set; }

        public string Pecas { get; set; }

        public decimal CustoTotal { get; set; }

        public DateTime DataInicio { get; set; }

        public DateTime? DataFim { get; set; }

        public bool Concluida { get; set; }

        public string TipoServico { get; set; }
    }
}
