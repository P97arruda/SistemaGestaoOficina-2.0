namespace SistemaGestaoOficina.DTO
{
    public class MobileReparacaoPecaDto
    {
        public string Nome { get; set; } = string.Empty;

        public int Quantidade { get; set; }

        public decimal PrecoUnitario { get; set; }

        public decimal Subtotal { get; set; }
    }
}
