using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StarConcessionaria.Application.Auth;
using StarConcessionaria.Domain.Entities;
using StarConcessionaria.Infrastructure.Persistence;

namespace StarConcessionaria.Infrastructure.Auth;

public static class AdminSeeder
{
    /// <summary>Cria o administrador inicial a partir de Seed:Admin:* se ainda nao houver nenhum.</summary>
    public static async Task SeedAdminAsync(this IServiceProvider services, IConfiguration configuration)
    {
        var email = configuration["Seed:Admin:Email"] 
                    ?? configuration["Seed__Admin__Email"] 
                    ?? configuration["SEED_ADMIN_EMAIL"] 
                    ?? "admin@star.com";

        var senha = configuration["Seed:Admin:Senha"] 
                    ?? configuration["Seed__Admin__Senha"] 
                    ?? configuration["SEED_ADMIN_SENHA"] 
                    ?? "Admin@123456";
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(senha)) return;

        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("AdminSeeder");

        email = email.Trim().ToLowerInvariant();
        if (await db.Usuarios.AnyAsync(u => u.Email.ToLower() == email)) return;

        db.Usuarios.Add(new UsuarioSistema
        {
            Nome = configuration["Seed:Admin:Nome"] ?? "Administrador",
            Email = email,
            SenhaHash = hasher.Hash(senha),
            Perfil = "administrador"
        });
        await db.SaveChangesAsync();
        logger.LogInformation("Administrador inicial criado: {Email}", email);
    }
}
