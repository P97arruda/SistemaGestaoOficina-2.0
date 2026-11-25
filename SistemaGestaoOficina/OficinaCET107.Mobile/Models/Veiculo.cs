using System;
using System.Collections.Generic;
using System.Text;

namespace OficinaCET107.Mobile.Models
{
    public class Veiculo
    {
        public int Id { get; set; }

        public string Marca { get; set; } = string.Empty;

        public string Modelo { get; set; } = string.Empty;

        public string Matricula { get; set; } = string.Empty;

        public int Ano { get; set; }

        public int Quilometragem { get; set; }

        public string Combustivel { get; set; } = string.Empty;
    }
}
