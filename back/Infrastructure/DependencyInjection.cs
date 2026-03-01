using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Application.Abstractions.Persistence;
using Application.Abstractions.Services.Utils;
using Infrastructure.Auth;
using Infrastructure.Persistence;
using Infrastructure.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

        services.AddHttpContextAccessor();
        
        services.AddDbContext<CrmDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
            options.UseSnakeCaseNamingConvention();
            
            options.ConfigureWarnings(w => 
                w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
        });
        
        services.AddScoped<ICrmDbContext>(provider => provider.GetRequiredService<CrmDbContext>());
        services.AddScoped<IDbInitializer, DbInitializer>();

        services.AddSingleton<IHasher, Hasher>();
        services.AddSingleton<IPasswordGenerator, PasswordGenerator>();
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<ICurrentUserContext, CurrentUserContext>();

        var jwtSettings = configuration.GetSection("JwtSettings");
        
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.MapInboundClaims = false; 

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidAudience = jwtSettings["Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtSettings["Secret"]!)),
                
                NameClaimType = "sub",
                RoleClaimType = "role" 
            };
        });

        services.AddAuthorization();
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
        services.AddScoped<IAuthorizationHandler, PermissionHandler>();
        
        return services;
    }
}
