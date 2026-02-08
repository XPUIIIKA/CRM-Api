using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Application.Abstractions.Persistence;
using Application.Abstractions.Services.Utils;
using Infrastructure.Auth;
using Infrastructure.Persistence;
using Infrastructure.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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

            options.Events = new JwtBearerEvents
            {
                OnTokenValidated = context =>
                {
                    var claims = context.Principal?.Claims.Select(c => $"{c.Type}: {c.Value}");
                    var isSystemAdmin = context.Principal?.IsInRole("SystemAdmin") ?? false;
                    
                    Console.WriteLine("--- Token Validation Success ---");
                    Console.WriteLine(string.Join("\n", claims ?? Array.Empty<string>()));
                    Console.WriteLine($"--- IsInRole('SystemAdmin'): {isSystemAdmin}");
                    
                    return Task.CompletedTask;
                },
                OnAuthenticationFailed = context =>
                {
                    Console.WriteLine($"--- Auth Failed: {context.Exception.Message}");
                    return Task.CompletedTask;
                }
            };
        });

        return services;
    }
}