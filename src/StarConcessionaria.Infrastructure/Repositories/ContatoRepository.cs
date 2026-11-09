using System.Globalization;
using Microsoft.EntityFrameworkCore;
using StarConcessionaria.Application.Common;
using StarConcessionaria.Application.Contatos;
using StarConcessionaria.Application.Veiculos;
using StarConcessionaria.Domain.Entities;
using StarConcessionaria.Infrastructure.Persistence;

namespace StarConcessionaria.Infrastructure.Repositories;

public class ContatoRepository : IContatoRepository
{
    private readonly ApplicationDbContext _db;

    public ContatoRepository(ApplicationDbContext db) => _db = db;

    public async Task<Resultado<ContatoResponse>> RegistrarContatoAsync(
        string tipo, string nome, string emailRaw, string telefoneRaw, string mensagem, int? veiculoId, int? unidadeId, CancellationToken ct = default)
    {
        Veiculo? veiculo = null;
        if (veiculoId.HasValue)
        {
            veiculo = await _db.Veiculos.AsNoTracking().FirstOrDefaultAsync(v => v.Id == veiculoId.Value, ct);
            if (veiculo is null)
                return Resultado<ContatoResponse>.Falha(TipoErro.NaoEncontrado, "Veiculo informado nao existe.");
        }

        var lojaEfetivaId = unidadeId ?? veiculo?.LojaId;
        Loja? loja = null;
        if (lojaEfetivaId.HasValue)
        {
            loja = await _db.Lojas.AsNoTracking().FirstOrDefaultAsync(l => l.Id == lojaEfetivaId.Value, ct);
            if (loja is null)
                return Resultado<ContatoResponse>.Falha(TipoErro.NaoEncontrado, "Unidade informada nao existe.");
        }

        var email = emailRaw.Trim().ToLowerInvariant();
        var telefone = telefoneRaw.Trim();
        var cliente = await _db.Clientes.FirstOrDefaultAsync(c => c.Email.ToLower() == email && c.Telefone == telefone, ct);
        if (cliente is null)
        {
            cliente = new Cliente { Nome = nome.Trim(), Email = email, Telefone = telefone };
            _db.Clientes.Add(cliente);
            await _db.SaveChangesAsync(ct);
        }

        var agora = DateTimeOffset.UtcNow;
        var contato = new Contato
        {
            ClienteId = cliente.Id,
            VeiculoId = veiculo?.Id,
            LojaId = loja?.Id,
            Tipo = tipo,
            Mensagem = mensagem,
            Data = agora
        };

        _db.Contatos.Add(contato);
        await _db.SaveChangesAsync(ct);

        return Resultado<ContatoResponse>.Ok(Mapear(contato, cliente, veiculo, loja, null));
    }

    public async Task<(IReadOnlyList<ContatoResponse> Itens, int Total)> ListarAsync(ContatoQuery f, CancellationToken ct = default)
    {
        var query = _db.Contatos.AsNoTracking()
            .Include(c => c.Cliente).Include(c => c.Veiculo).Include(c => c.Loja).Include(c => c.Usuario)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(f.Tipo))
            query = query.Where(c => c.Tipo.ToLower() == f.Tipo.ToLower());
        if (f.UnidadeId.HasValue)
            query = query.Where(c => c.LojaId == f.UnidadeId.Value);
        if (f.Respondido.HasValue)
        {
            query = f.Respondido.Value
                ? query.Where(c => c.UsuarioId != null)
                : query.Where(c => c.UsuarioId == null);
        }

        var total = await query.CountAsync(ct);
        var itens = await query
            .OrderByDescending(c => c.Id)
            .Skip((f.Pagina - 1) * f.ItensPorPagina)
            .Take(f.ItensPorPagina)
            .ToListAsync(ct);

        return (itens.Select(c => Mapear(c, c.Cliente, c.Veiculo, c.Loja, c.Usuario)).ToList(), total);
    }

    public async Task<Resultado<ContatoResponse>> ResponderAsync(int id, string notasResposta, UsuarioAtual usuario, CancellationToken ct = default)
    {
        var contato = await _db.Contatos
            .Include(c => c.Cliente).Include(c => c.Veiculo).Include(c => c.Loja)
            .FirstOrDefaultAsync(c => c.Id == id, ct);

        if (contato is null)
            return Resultado<ContatoResponse>.Falha(TipoErro.NaoEncontrado, "Contato nao encontrado.");

        contato.UsuarioId = usuario.Id;
        contato.Mensagem += $"\n\n[Atendimento por {usuario.Nome} em {DateTimeOffset.UtcNow:yyyy-MM-dd HH:mm} UTC]: {notasResposta}";
        await _db.SaveChangesAsync(ct);

        var usu = await _db.Usuarios.AsNoTracking().FirstOrDefaultAsync(u => u.Id == usuario.Id, ct);
        return Resultado<ContatoResponse>.Ok(Mapear(contato, contato.Cliente, contato.Veiculo, contato.Loja, usu));
    }

    private static ContatoResponse Mapear(Contato c, Cliente? cli, Veiculo? v, Loja? l, UsuarioSistema? u)
    {
        var data = c.Data ?? DateTimeOffset.UtcNow;
        var prefixo = c.Tipo == TipoContato.InicioCompra ? "PROP" : "CON";
        var protocolo = $"{prefixo}-{data.Year}-{c.Id.ToString("D6", CultureInfo.InvariantCulture)}";
        var vDesc = v is null ? null : $"{v.Marca} {v.Modelo} ({v.Ano})";
        return new ContatoResponse(
            c.Id, protocolo, c.Tipo, cli?.Id, cli?.Nome, cli?.Email, cli?.Telefone,
            v?.Id, vDesc, l?.Id, l?.Nome, u?.Id, u?.Nome, c.UsuarioId is not null, c.Mensagem, data);
    }
}
