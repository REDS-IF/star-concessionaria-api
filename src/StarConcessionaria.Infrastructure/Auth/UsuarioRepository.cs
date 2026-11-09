using Microsoft.EntityFrameworkCore;
using StarConcessionaria.Application.Auth;
using StarConcessionaria.Domain.Entities;
using StarConcessionaria.Infrastructure.Persistence;

namespace StarConcessionaria.Infrastructure.Auth;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly ApplicationDbContext _db;

    public UsuarioRepository(ApplicationDbContext db) => _db = db;

    public Task<UsuarioSistema?> ObterPorEmailAsync(string email, CancellationToken ct = default) =>
        _db.Usuarios.AsNoTracking().FirstOrDefaultAsync(u => u.Email.ToLower() == email, ct);

    public Task<UsuarioSistema?> ObterPorIdAsync(int id, CancellationToken ct = default) =>
        _db.Usuarios.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id, ct);
}
