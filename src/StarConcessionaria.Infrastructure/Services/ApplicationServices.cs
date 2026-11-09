using Microsoft.EntityFrameworkCore;
using StarConcessionaria.Application.DTOs;
using StarConcessionaria.Application.Interfaces;
using StarConcessionaria.Domain.Entities;
using StarConcessionaria.Domain.Enums;
using StarConcessionaria.Domain.Exceptions;
using StarConcessionaria.Infrastructure.Persistence;
using StarConcessionaria.Infrastructure.Security;

namespace StarConcessionaria.Application.Services;

public class AuthService : IAuthService
{
    private readonly StarConcessionariaDbContext _context;
    private readonly IPasswordHasher _hasher;
    private readonly ITokenService _tokenService;

    public AuthService(StarConcessionariaDbContext context, IPasswordHasher hasher, ITokenService tokenService)
    {
        _context = context;
        _hasher = hasher;
        _tokenService = tokenService;
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.EmailUsu == request.Email);
        if (usuario == null) return null;

        if (!_hasher.Verify(request.Senha, usuario.SenhaUsu))
            return null;

        var token = _tokenService.GenerateToken(usuario);
        return new LoginResponse(token, usuario.NomeUsu, usuario.EmailUsu, usuario.PerfilUsu);
    }
}

public class UnidadeService : IUnidadeService
{
    private readonly StarConcessionariaDbContext _context;

    public UnidadeService(StarConcessionariaDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<LojaResponse>> ObterLojasAsync()
    {
        return await _context.Lojas
            .AsNoTracking()
            .Select(l => new LojaResponse(
                l.IdLoj,
                l.NomeLoj,
                l.TelefoneLoj,
                l.EnderecoLoj,
                l.NumeroLoj,
                l.BairroLoj,
                l.CidadeLoj,
                l.EstadoLoj,
                l.LatitudeLoj,
                l.LongitudeLoj
            ))
            .ToListAsync();
    }

    public async Task<LojaResponse?> ObterLojaPorIdAsync(int id)
    {
        var l = await _context.Lojas.FindAsync(id);
        if (l == null) return null;

        return new LojaResponse(
            l.IdLoj,
            l.NomeLoj,
            l.TelefoneLoj,
            l.EnderecoLoj,
            l.NumeroLoj,
            l.BairroLoj,
            l.CidadeLoj,
            l.EstadoLoj,
            l.LatitudeLoj,
            l.LongitudeLoj
        );
    }

    public async Task<LojaResponse> CriarLojaAsync(CriarLojaRequest request)
    {
        var loja = new Loja
        {
            NomeLoj = request.NomeLoj,
            TelefoneLoj = request.TelefoneLoj,
            CepLoj = request.CepLoj,
            EnderecoLoj = request.EnderecoLoj,
            NumeroLoj = request.NumeroLoj,
            ComplementoLoj = request.ComplementoLoj,
            BairroLoj = request.BairroLoj,
            CidadeLoj = request.CidadeLoj,
            EstadoLoj = request.EstadoLoj,
            LatitudeLoj = request.LatitudeLoj,
            LongitudeLoj = request.LongitudeLoj
        };

        _context.Lojas.Add(loja);
        await _context.SaveChangesAsync();

        return new LojaResponse(
            loja.IdLoj,
            loja.NomeLoj,
            loja.TelefoneLoj,
            loja.EnderecoLoj,
            loja.NumeroLoj,
            loja.BairroLoj,
            loja.CidadeLoj,
            loja.EstadoLoj,
            loja.LatitudeLoj,
            loja.LongitudeLoj
        );
    }
}

public class VeiculoService : IVeiculoService
{
    private readonly StarConcessionariaDbContext _context;

    public VeiculoService(StarConcessionariaDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<VeiculoCatalogoResponse>> ObterCatalogoAsync(FiltroVeiculosRequest filtros)
    {
        var query = _context.Veiculos
            .Include(v => v.Loja)
            .Include(v => v.Fotos)
            .AsNoTracking()
            .AsQueryable();

        // RF06 e RF07 - Filtros simultâneos
        if (!string.IsNullOrWhiteSpace(filtros.Marca))
            query = query.Where(v => v.MarcaVei.ToLower().Contains(filtros.Marca.ToLower()));

        if (!string.IsNullOrWhiteSpace(filtros.Modelo))
            query = query.Where(v => v.ModeloVei.ToLower().Contains(filtros.Modelo.ToLower()));

        if (filtros.PrecoMin.HasValue)
            query = query.Where(v => v.PrecoVei >= filtros.PrecoMin.Value);

        if (filtros.PrecoMax.HasValue)
            query = query.Where(v => v.PrecoVei <= filtros.PrecoMax.Value);

        if (filtros.AnoMin.HasValue)
            query = query.Where(v => v.AnoVei >= filtros.AnoMin.Value);

        if (filtros.AnoMax.HasValue)
            query = query.Where(v => v.AnoVei <= filtros.AnoMax.Value);

        if (!string.IsNullOrWhiteSpace(filtros.Tipo))
            query = query.Where(v => v.TipoVei == filtros.Tipo);

        if (filtros.LojaId.HasValue)
            query = query.Where(v => v.IdLojFk == filtros.LojaId.Value);

        return await query.Select(v => new VeiculoCatalogoResponse(
            v.IdVei,
            v.IdLojFk,
            v.Loja != null ? v.Loja.NomeLoj : "Unidade Star",
            v.TipoVei,
            v.MarcaVei,
            v.ModeloVei,
            v.AnoVei,
            v.PrecoVei,
            v.QuilometragemVei,
            v.CorVei,
            v.SituacaoVei,
            v.Fotos.Select(f => f.UrlFot).FirstOrDefault()
        )).ToListAsync();
    }

    public async Task<Veiculo?> ObterDetalhesAsync(int id)
    {
        return await _context.Veiculos
            .Include(v => v.Loja)
            .Include(v => v.Fotos)
            .Include(v => v.Alteracoes)
            .FirstOrDefaultAsync(v => v.IdVei == id);
    }

    public async Task<Veiculo> CriarVeiculoAsync(CriarVeiculoRequest request)
    {
        if (request.PrecoVei <= 0)
            throw new DomainException("O preço do veículo deve ser maior que zero.");

        var lojaExiste = await _context.Lojas.AnyAsync(l => l.IdLoj == request.IdLojFk);
        if (!lojaExiste)
            throw new DomainException("Loja física (IdLojFk) não encontrada.");

        var veiculo = new Veiculo
        {
            IdLojFk = request.IdLojFk,
            TipoVei = request.TipoVei,
            MarcaVei = request.MarcaVei,
            ModeloVei = request.ModeloVei,
            AnoVei = request.AnoVei,
            PrecoVei = request.PrecoVei,
            QuilometragemVei = request.QuilometragemVei,
            CorVei = request.CorVei,
            DescricaoVei = request.DescricaoVei,
            SituacaoVei = SituacaoVeiculo.Disponivel
        };

        _context.Veiculos.Add(veiculo);
        await _context.SaveChangesAsync();

        return veiculo;
    }

    public async Task<bool> AtualizarSituacaoAsync(int id, string novaSituacao, string? justificativa = null)
    {
        var veiculo = await _context.Veiculos.FindAsync(id);
        if (veiculo == null) return false;

        veiculo.SituacaoVei = novaSituacao;

        if (!string.IsNullOrWhiteSpace(justificativa))
        {
            _context.AlteracoesVeiculo.Add(new AlteracaoVeiculo
            {
                IdVeiFk = id,
                DescricaoAlt = $"Alteração de situação para {novaSituacao}: {justificativa}"
            });
        }

        await _context.SaveChangesAsync();
        return true;
    }
}

public class FaqService : IFaqService
{
    private readonly StarConcessionariaDbContext _context;

    public FaqService(StarConcessionariaDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<FaqResponse>> ObterFaqsAsync()
    {
        return await _context.DuvidasFrequentes
            .AsNoTracking()
            .Select(d => new FaqResponse(d.IdDf, d.PerguntaDf, d.RespostaDf))
            .ToListAsync();
    }
}
