using System;
using System.Collections.Generic;
using System.Text;

namespace OficinaCET107.Mobile.Models
{
    public class ReparacaoPeca
    {
        public string Nome { get; set; } = string.Empty;
        public int Quantidade { get; set; }
        public decimal PrecoUnitario { get; set; }
        public decimal Subtotal { get; set; }
    }
}
