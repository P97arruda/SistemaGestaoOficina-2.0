namespace SistemaGestaoOficina.DTO
{
    public class MobileMarcacaoDto
    {
        public int Id { get; set; }

        public string TipoServico { get; set; } = string.Empty;

        public DateTime DataHora { get; set; }

        public string Estado { get; set; } = string.Empty;

        public string Veiculo { get; set; } = string.Empty;

        public string Matricula { get; set; } = string.Empty;

        public string Mecanico { get; set; } = string.Empty;
    }
}
