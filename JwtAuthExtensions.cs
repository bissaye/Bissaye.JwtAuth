using Bissaye.JwtAuth.Models;
using Bissaye.JwtAuth.Services;
using Bissaye.JwtAuth.Services.Concretes;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bissaye.JwtAuth
{
    public static class JwtAuthExtensions
    {
        public static void AddBissayeJwtAuth(this IServiceCollection services, 
            IConfiguration configuration, 
            Action<JwtBearerEvents>? configureJwtBearerEvents = null,
            Action<CookieAuthenticationOptions>? configureJwtCookie = null)
        {
            services.Configure<JwtAuthConfigs>(configuration.GetSection("JwtAuthConfigs"));
            
            services.AddScoped<ITokenServices, TokenServices>();

            var serviceProvider = services.BuildServiceProvider();
            var jwtAuthConfigs = serviceProvider.GetRequiredService<IOptions<JwtAuthConfigs>>().Value;
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtAuthConfigs.SecretKey));

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = jwtAuthConfigs.Issuer,
                        ValidateAudience = true,
                        ValidAudience = jwtAuthConfigs.Audience,
                        ValidateLifetime = true,
                        IssuerSigningKey = key,
                        ValidateIssuerSigningKey = true,
                        ClockSkew = TimeSpan.Zero
                    };

                    if (configureJwtBearerEvents != null)
                    {
                        options.Events = new JwtBearerEvents();
                        configureJwtBearerEvents.Invoke(options.Events);
                    }

                    if (configureJwtCookie != null)
                    {
                        services.AddAuthentication()
                            .AddCookie("JwtCookieScheme", configureJwtCookie);
                    }
                });
        }
    }
}
