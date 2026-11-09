using System.ComponentModel.DataAnnotations;

namespace StarConcessionaria.Application.Auth;

public record LoginRequest(
    [Required, EmailAddress, StringLength(300)] string Email,
    [Required, StringLength(200, MinimumLength = 1)] string Senha);

public record UsuarioResponse(int Id, string Nome, string Email, string Perfil);

public record LoginResponse(string Token, DateTimeOffset ExpiraEm, UsuarioResponse Usuario);
