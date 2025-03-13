using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Bissaye.JwtAuth.Services
{
    public interface ITokenServices
    {
        public bool CheckRefreshToken(ClaimsPrincipal User);
        public T GetClaimsValue<T>(ClaimsPrincipal User) where T : new ();
        public List<Claim> GetClaims<T>(T obj) where T : class;
        public string GenerateToken(List<Claim> claims, bool refresh);
    }
}
