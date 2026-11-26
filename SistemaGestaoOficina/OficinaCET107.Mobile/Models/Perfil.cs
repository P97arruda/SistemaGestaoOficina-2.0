using System;
using System.Collections.Generic;
using System.Text;

namespace OficinaCET107.Mobile.Models
{
    public class Perfil
    {
        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string NIF { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        public string? FotoUrl { get; set; }
    }
}
