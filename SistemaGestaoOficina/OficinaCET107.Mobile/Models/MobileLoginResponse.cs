using System;
using System.Collections.Generic;
using System.Text;

namespace OficinaCET107.Mobile.Models
{
    internal class MobileLoginResponse
    {
        public string TokenType { get; set; } = string.Empty;

        public string AccessToken { get; set; } = string.Empty;

        public int ExpiresIn { get; set; }

        public string RefreshToken { get; set; } = string.Empty;
    }
}
