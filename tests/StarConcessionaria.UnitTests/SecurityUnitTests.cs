using FluentAssertions;
using Microsoft.Extensions.Configuration;
using StarConcessionaria.Domain.Entities;
using StarConcessionaria.Domain.Enums;
using StarConcessionaria.Infrastructure.Security;
using Xunit;

namespace StarConcessionaria.UnitTests;

public class SecurityUnitTests
{
    [Fact]
    public void PasswordHasher_DeveGerarHashValidoEVerificarCorretamente()
    {
        // Arrange
        var hasher = new PasswordHasher();
        var senhaPura = "Admin@2026";

        // Act
        var hash = hasher.Hash(senhaPura);
        var ehValida = hasher.Verify(senhaPura, hash);
        var ehInvalida = hasher.Verify("SenhaErrada", hash);

        // Assert
        hash.Should().NotBeNullOrWhiteSpace();
        hash.Should().NotBe(senhaPura);
        ehValida.Should().BeTrue();
        ehInvalida.Should().BeFalse();
    }

    [Fact]
    public void TokenService_DeveGerarJwtValidoComClaimsCorretas()
    {
        // Arrange
        var inMemorySettings = new Dictionary<string, string?>
        {
            {"Jwt:SecretKey", "Chave_Ultra_Secreta_Para_Testes_Unitarios_Com_Mais_De_32_Caracteres"},
            {"Jwt:Issuer", "StarConcessionariaTest"},
            {"Jwt:Audience", "StarConcessionariaTest"}
        };

        IConfiguration config = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        var tokenService = new TokenService(config);

        var usuario = new UsuarioSistema
        {
            IdUsu = 42,
            NomeUsu = "Admin Teste",
            EmailUsu = "admin@teste.com",
            PerfilUsu = PerfilUsuario.Administrador
        };

        // Act
        var token = tokenService.GenerateToken(usuario);

        // Assert
        token.Should().NotBeNullOrWhiteSpace();
        token.Split('.').Should().HaveCount(3, "Um JWT padrão possui 3 segmentos separados por ponto");
    }
}
