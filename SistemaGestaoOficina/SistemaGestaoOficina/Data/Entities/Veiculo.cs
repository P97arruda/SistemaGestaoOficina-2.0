namespace SistemaGestaoOficina.Data.Entities
{
    public class Veiculo
    {
        public int Id { get; set; }

        public string Marca { get; set; }

        public string Modelo { get; set; }

        public string Matricula { get; set; }

        public int Ano { get; set; }

        public int Quilometragem { get; set; }

        public string Combustivel { get; set; }
    }
}
