using System.Globalization;
using StarConcessionaria.Application.Common;
using StarConcessionaria.Domain.Entities;

namespace StarConcessionaria.Application.Veiculos;

public class VeiculoService : IVeiculoService
{
    private readonly IVeiculoRepository _repository;

    public VeiculoService(IVeiculoRepository repository) => _repository = repository;

    private const int ItensPorPaginaMaximo = 50;

    public async Task<Resultado<PagedResult<VeiculoCatalogoResponse>>> ListarCatalogoAsync(CatalogoQuery filtro, CancellationToken ct = default)
    {
        if (filtro.PrecoMin < 0 || filtro.PrecoMax < 0 || filtro.PrecoMin > filtro.PrecoMax)
            return Resultado<PagedResult<VeiculoCatalogoResponse>>.Falha(TipoErro.Invalido, "Faixa de preco invalida.");
        if (filtro.AnoMin > filtro.AnoMax)
            return Resultado<PagedResult<VeiculoCatalogoResponse>>.Falha(TipoErro.Invalido, "Faixa de ano invalida.");

        var pagina = Math.Max(1, filtro.Pagina);
        var itens = Math.Clamp(filtro.ItensPorPagina, 1, ItensPorPaginaMaximo);

        var normalizado = new CatalogoQuery
        {
            Marca = filtro.Marca?.Trim(),
            Modelo = filtro.Modelo?.Trim(),
            Tipo = filtro.Tipo?.Trim(),
            PrecoMin = filtro.PrecoMin,
            PrecoMax = filtro.PrecoMax,
            AnoMin = filtro.AnoMin,
            AnoMax = filtro.AnoMax,
            UnidadeId = filtro.UnidadeId,
            SomenteDisponiveis = filtro.SomenteDisponiveis,
            Pagina = pagina,
            ItensPorPagina = itens
        };

        var (lista, total) = await _repository.ListarCatalogoAsync(normalizado, ct);
        return Resultado<PagedResult<VeiculoCatalogoResponse>>.Ok(new PagedResult<VeiculoCatalogoResponse>(lista, pagina, itens, total));
    }

    public async Task<VeiculoDetalheResponse?> ObterAsync(int id, CancellationToken ct = default)
    {
        var veiculo = await _repository.ObterDetalheAsync(id, ct);
        return veiculo is null ? null : Mapear(veiculo);
    }

    public async Task<Resultado<VeiculoDetalheResponse>> CriarAsync(SalvarVeiculoRequest request, UsuarioAtual usuario, CancellationToken ct = default)
    {
        if (!await _repository.LojaExisteAsync(request.LojaId, ct))
            return Resultado<VeiculoDetalheResponse>.Falha(TipoErro.Invalido, "Loja informada nao existe.");

        var veiculo = new Veiculo { Situacao = SituacaoVeiculo.Disponivel };
        Aplicar(veiculo, request);
        await _repository.AdicionarAsync(veiculo, ct);
        await _repository.SalvarAsync(ct); // gera o Id

        await _repository.AdicionarAlteracaoAsync(new AlteracaoVeiculo
        {
            VeiculoId = veiculo.Id,
            Descricao = $"Cadastro do veiculo por {usuario.Nome} (usuario {usuario.Id})."
        }, ct);
        await _repository.SalvarAsync(ct);

        var criado = await _repository.ObterDetalheAsync(veiculo.Id, ct);
        return Resultado<VeiculoDetalheResponse>.Ok(Mapear(criado!));
    }

    public async Task<Resultado<VeiculoDetalheResponse>> AtualizarAsync(int id, SalvarVeiculoRequest request, UsuarioAtual usuario, CancellationToken ct = default)
    {
        var veiculo = await _repository.ObterParaAtualizacaoAsync(id, ct);
        if (veiculo is null)
            return Resultado<VeiculoDetalheResponse>.Falha(TipoErro.NaoEncontrado, "Veiculo nao encontrado.");

        if (request.LojaId != veiculo.LojaId && !await _repository.LojaExisteAsync(request.LojaId, ct))
            return Resultado<VeiculoDetalheResponse>.Falha(TipoErro.Invalido, "Loja informada nao existe.");

        var antes = Snapshot(veiculo);
        Aplicar(veiculo, request);
        var depois = Snapshot(veiculo);

        var diffs = depois.Where(kv => antes[kv.Key] != kv.Value)
            .Select(kv => $"{kv.Key}: '{antes[kv.Key]}' -> '{kv.Value}'").ToList();

        if (diffs.Count > 0)
        {
            await _repository.AdicionarAlteracaoAsync(new AlteracaoVeiculo
            {
                VeiculoId = veiculo.Id,
                Descricao = $"Atualizado por {usuario.Nome} (usuario {usuario.Id}): {string.Join("; ", diffs)}"
            }, ct);
            await _repository.SalvarAsync(ct);
        }

        var atualizado = await _repository.ObterDetalheAsync(id, ct);
        return Resultado<VeiculoDetalheResponse>.Ok(Mapear(atualizado!));
    }

    public async Task<Resultado<VeiculoDetalheResponse>> AlterarSituacaoAsync(int id, AlterarSituacaoRequest request, UsuarioAtual usuario, CancellationToken ct = default)
    {
        var nova = SituacaoVeiculo.Normalizar(request.Situacao);
        if (nova is null)
            return Resultado<VeiculoDetalheResponse>.Falha(TipoErro.Invalido,
                $"Situacao invalida. Use: {string.Join(", ", SituacaoVeiculo.Validas)}.");

        var veiculo = await _repository.ObterParaAtualizacaoAsync(id, ct);
        if (veiculo is null)
            return Resultado<VeiculoDetalheResponse>.Falha(TipoErro.NaoEncontrado, "Veiculo nao encontrado.");

        if (veiculo.Situacao != nova)
        {
            var justificativa = string.IsNullOrWhiteSpace(request.Justificativa) ? "" : $" Justificativa: {request.Justificativa.Trim()}";
            await _repository.AdicionarAlteracaoAsync(new AlteracaoVeiculo
            {
                VeiculoId = veiculo.Id,
                Descricao = $"Situacao alterada por {usuario.Nome} (usuario {usuario.Id}): '{veiculo.Situacao}' -> '{nova}'.{justificativa}"
            }, ct);
            veiculo.Situacao = nova;
            await _repository.SalvarAsync(ct);
        }

        var atualizado = await _repository.ObterDetalheAsync(id, ct);
        return Resultado<VeiculoDetalheResponse>.Ok(Mapear(atualizado!));
    }

    public async Task<Resultado<VeiculoFotoResponse>> AdicionarFotoAsync(int veiculoId, AdicionarFotoRequest request, UsuarioAtual usuario, CancellationToken ct = default)
    {
        if (!Uri.TryCreate(request.Url.Trim(), UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            return Resultado<VeiculoFotoResponse>.Falha(TipoErro.Invalido, "Url da foto deve ser http(s) absoluta.");

        if (!await _repository.VeiculoExisteAsync(veiculoId, ct))
            return Resultado<VeiculoFotoResponse>.Falha(TipoErro.NaoEncontrado, "Veiculo nao encontrado.");

        // A primeira foto do veiculo sempre e a capa.
        var principal = request.Principal || await _repository.ContarFotosAsync(veiculoId, ct) == 0;

        var foto = new FotoVeiculo { VeiculoId = veiculoId, Url = request.Url.Trim(), Principal = principal, Ordem = request.Ordem };
        await _repository.AdicionarFotoAsync(foto, ct);
        await RegistrarAsync(veiculoId, $"Foto {foto.Id} adicionada por {usuario.Nome} (usuario {usuario.Id}){(principal ? " como capa" : "")}.", ct);
        return Resultado<VeiculoFotoResponse>.Ok(MapearFoto(foto));
    }

    public async Task<Resultado<bool>> RemoverFotoAsync(int veiculoId, int fotoId, UsuarioAtual usuario, CancellationToken ct = default)
    {
        var foto = await _repository.ObterFotoAsync(veiculoId, fotoId, ct);
        if (foto is null)
            return Resultado<bool>.Falha(TipoErro.NaoEncontrado, "Foto nao encontrada para este veiculo.");

        await _repository.RemoverFotoAsync(foto, ct);
        await RegistrarAsync(veiculoId, $"Foto {fotoId} removida por {usuario.Nome} (usuario {usuario.Id}).", ct);
        return Resultado<bool>.Ok(true);
    }

    public async Task<Resultado<VeiculoFotoResponse>> DefinirFotoPrincipalAsync(int veiculoId, int fotoId, UsuarioAtual usuario, CancellationToken ct = default)
    {
        var foto = await _repository.ObterFotoAsync(veiculoId, fotoId, ct);
        if (foto is null)
            return Resultado<VeiculoFotoResponse>.Falha(TipoErro.NaoEncontrado, "Foto nao encontrada para este veiculo.");

        if (!foto.Principal)
        {
            await _repository.DefinirFotoPrincipalAsync(foto, ct);
            await RegistrarAsync(veiculoId, $"Foto {fotoId} definida como capa por {usuario.Nome} (usuario {usuario.Id}).", ct);
        }
        return Resultado<VeiculoFotoResponse>.Ok(MapearFoto(foto));
    }

    private async Task RegistrarAsync(int veiculoId, string descricao, CancellationToken ct)
    {
        await _repository.AdicionarAlteracaoAsync(new AlteracaoVeiculo { VeiculoId = veiculoId, Descricao = descricao }, ct);
        await _repository.SalvarAsync(ct);
    }

    private static VeiculoFotoResponse MapearFoto(FotoVeiculo f) => new(f.Id, f.Url, f.Principal, f.Ordem);

    public async Task<Resultado<IReadOnlyList<VeiculoHistoricoResponse>>> ObterHistoricoAsync(int id, CancellationToken ct = default)
    {
        if (!await _repository.VeiculoExisteAsync(id, ct))
            return Resultado<IReadOnlyList<VeiculoHistoricoResponse>>.Falha(TipoErro.NaoEncontrado, "Veiculo nao encontrado.");

        var itens = await _repository.ListarHistoricoAsync(id, ct);
        return Resultado<IReadOnlyList<VeiculoHistoricoResponse>>.Ok(
            itens.Select(a => new VeiculoHistoricoResponse(a.Id, a.Descricao)).ToList());
    }

    private static void Aplicar(Veiculo v, SalvarVeiculoRequest r)
    {
        v.LojaId = r.LojaId;
        v.Tipo = string.Equals(r.Tipo.Trim(), "Novo", StringComparison.OrdinalIgnoreCase) ? "Novo" : "Seminovo";
        v.Marca = r.Marca.Trim();
        v.Modelo = r.Modelo.Trim();
        v.Ano = r.Ano;
        v.Preco = r.Preco;
        v.Quilometragem = r.Quilometragem;
        v.Cor = r.Cor.Trim();
        v.Descricao = r.Descricao?.Trim();
    }

    private static Dictionary<string, string> Snapshot(Veiculo v) => new()
    {
        ["LojaId"] = v.LojaId.ToString(CultureInfo.InvariantCulture),
        ["Tipo"] = v.Tipo,
        ["Marca"] = v.Marca,
        ["Modelo"] = v.Modelo,
        ["Ano"] = v.Ano.ToString(CultureInfo.InvariantCulture),
        ["Preco"] = v.Preco.ToString("0.00", CultureInfo.InvariantCulture),
        ["Quilometragem"] = v.Quilometragem.ToString(CultureInfo.InvariantCulture),
        ["Cor"] = v.Cor,
        ["Descricao"] = v.Descricao ?? ""
    };

    private static VeiculoDetalheResponse Mapear(Veiculo v) => new(
        v.Id, v.LojaId, v.Loja?.Nome, v.Tipo, v.Marca, v.Modelo, v.Ano, v.Preco, v.Quilometragem,
        v.Cor, v.Descricao, v.Situacao,
        v.Fotos.OrderByDescending(f => f.Principal).ThenBy(f => f.Ordem).ThenBy(f => f.Id).Select(MapearFoto).ToList());
}
