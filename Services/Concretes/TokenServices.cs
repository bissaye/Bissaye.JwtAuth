using Bissaye.JwtAuth.Models;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace Bissaye.JwtAuth.Services.Concretes
{
    public class TokenServices : ITokenServices
    {
        private readonly JwtAuthConfigs _options;
        private readonly SymmetricSecurityKey _key;
        private readonly ILogger<TokenServices> _logger;
        public TokenServices(IOptions<JwtAuthConfigs> options, ILogger<TokenServices> logger)
        {
            _options = options.Value;
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey));
            _logger = logger;
        }


        public string GenerateToken(List<Claim> claims, bool refresh = false)
        {
            if (refresh)
            {
                claims.Add(new Claim("refresh", "1"));
            }

            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();

            DateTime now = DateTime.UtcNow;

            SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Issuer = _options.Issuer,
                Audience = _options.Audience,
                Expires = now.AddMinutes(refresh ? _options.RefreshTokenExpirationMinutes : _options.AccessTokenExpirationMinutes),
                SigningCredentials = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256)
            };

            SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);

            string encodedJwt = tokenHandler.WriteToken(token);

            _logger.LogInformation("Token généré avec succès. Expiration : {Expiration}", tokenDescriptor.Expires);

            return encodedJwt;
        }

        public List<Claim> GetClaims<T>(T obj) where T : class
        {
            List<Claim> claims = new();

            PropertyInfo[] properties = typeof(T).GetProperties();

            _logger.LogInformation("Extraction des claims depuis l'objet de type {Type}", typeof(T).Name);

            foreach (PropertyInfo property in properties) {
                var value = property.GetValue(obj)?.ToString();
                if (!string.IsNullOrEmpty(value))
                {
                    claims.Add(new Claim(property.Name, value));
                    _logger.LogInformation("Ajout du claim : {Key} = {Value}", property.Name, value);
                }
            }
            
            return claims;
        }

        public T GetClaimsValue<T>(ClaimsPrincipal User) where T : new()
        {
            T obj = new ();

            PropertyInfo[] properties = typeof(T).GetProperties();

            _logger.LogInformation("Conversion des claims en objet de type {Type}", typeof(T).Name);

            foreach (PropertyInfo property in properties)
            {
                var claim = User.Claims.FirstOrDefault(claim => claim.Type == property.Name);
                if (claim != null && property.CanWrite)
                {
                    try
                    {
                        object? convertedValue = ConvertToType(claim.Value, property.PropertyType);
                        if (convertedValue != null)
                        {
                            property.SetValue(obj, convertedValue);
                            _logger.LogInformation("Claim {Key} converti avec succès en {Type}", property.Name, property.PropertyType.Name);
                        }
                        else
                        {
                            _logger.LogWarning("Impossible de convertir le claim {Key} en {Type}", property.Name, property.PropertyType.Name);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Erreur lors de la conversion du claim {Key} en {Type}, message: {errorMessage}, stackTrace: {stackTrace}", property.Name, property.PropertyType.Name, ex.Message, ex.StackTrace);
                    }
                }
            }
            int a = 1;
            return obj;
        }

        public bool CheckRefreshToken(ClaimsPrincipal User)
        {
            var refreshClaim = User.Claims.FirstOrDefault(s => s.Type == "refresh");
            var isRefreshToken = refreshClaim != null && refreshClaim.Value == "1";

            _logger.LogInformation("Vérification du token de rafraîchissement : {Status}", isRefreshToken ? "Valide" : "Invalide");

            return isRefreshToken;
        }

        private object? ConvertToType(string value, Type targetType)
        {
            try 
            { 
                if (targetType == typeof(Guid))
                {
                    return Guid.TryParse(value, out Guid guidValue) ? guidValue : null;
                }
                if (targetType == typeof(int))
                {
                    return int.TryParse(value, out int intValue) ? intValue : null;
                }
                if (targetType == typeof(bool))
                {
                    return bool.TryParse(value, out bool boolValue) ? boolValue : null;
                }
                if (targetType == typeof(DateTime))
                {
                    return DateTime.TryParse(value, out DateTime dateValue) ? dateValue : null;
                }
                if (targetType.IsEnum)
                {
                    return Enum.TryParse(targetType, value, out object? enumValue) ? enumValue : null;
                }

                return Convert.ChangeType(value, targetType);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la conversion de la valeur '{Value}' en {Type} ", value, targetType.Name);
                return null;
            }
        }
    }
}
