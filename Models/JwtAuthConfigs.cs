using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Bissaye.JwtAuth.Models
{
    public class JwtAuthConfigs
    {
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
        public int AccessTokenExpirationMinutes { get; set; } = 1440; // 24h par défaut
        public int RefreshTokenExpirationMinutes { get; set; } = 43200; // 30 jours par défaut
    }
}
