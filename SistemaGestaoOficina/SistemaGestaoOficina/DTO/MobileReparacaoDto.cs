namespace SistemaGestaoOficina.DTO
{
    public class MobileReparacaoDto
    {
        public int Id { get; set; }
        public string TipoServico { get; set; } = string.Empty;
        public DateTime DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
        public string Veiculo { get; set; } = string.Empty;
        public string Matricula { get; set; } = string.Empty;
        public decimal CustoMaoDeObra { get; set; }
        public decimal CustoTotal { get; set; }
    }
}
