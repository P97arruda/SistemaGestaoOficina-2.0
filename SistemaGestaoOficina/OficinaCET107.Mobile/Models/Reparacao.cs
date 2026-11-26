using System;
using System.Collections.Generic;
using System.Text;

namespace OficinaCET107.Mobile.Models
{
    public class Reparacao
    {
        public int Id { get; set; }

        public string TipoServico { get; set; } = string.Empty;

        public DateTime DataInicio { get; set; }

        public DateTime? DataFim { get; set; }

        public string Veiculo { get; set; } = string.Empty;

        public string Matricula { get; set; } = string.Empty;

        public decimal CustoMaoDeObra { get; set; }

        public decimal CustoTotal { get; set; }

        // MECÂNICOS RESPONSÁVEIS
        public List<string> Mecanicos { get; set; } = new();

        // PEÇAS UTILIZADAS
        public List<ReparacaoPeca> Pecas { get; set; } = new();
    }
}
