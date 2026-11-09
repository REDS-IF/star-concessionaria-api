using System.Globalization;
using StarConcessionaria.Application.Common;
using StarConcessionaria.Domain.Entities;

namespace StarConcessionaria.Application.Propostas;

/// <summary>
/// RN06: a plataforma NAO conclui a compra nem processa pagamento. Registra um contato comercial
/// (tipo_con = 'InicioCompra') e devolve um protocolo para atendimento na loja.
/// </summary>
public class PropostaService : IPropostaService
{
    public const string TipoContatoInicioCompra = "InicioCompra";

    private readonly IPropostaRepository _repository;

    public PropostaService(IPropostaRepository repository) => _repository = repository;

    public async Task<Resultado<PropostaResponse>> SolicitarAsync(SolicitarPropostaRequest r, CancellationToken ct = default)
    {
        var veiculo = await _repository.ObterVeiculoAsync(r.VeiculoId, ct);
        if (veiculo is null)
            return Falha(TipoErro.NaoEncontrado, "Veiculo nao encontrado.");

        // RN02: veiculo vendido nao recebe propostas.
        if (veiculo.Situacao == SituacaoVeiculo.Vendido)
            return Falha(TipoErro.Conflito, "Veiculo ja foi vendido.");

        var lojaId = r.UnidadeId ?? veiculo.LojaId;
        if (r.UnidadeId is not null && !await _repository.LojaExisteAsync(lojaId, ct))
            return Falha(TipoErro.Invalido, "Unidade informada nao existe.");

        var forma = await _repository.ObterFormaPagamentoAsync(r.FormaPagamento.Trim(), ct);
        if (forma is null)
            return Falha(TipoErro.Invalido, "Forma de pagamento nao aceita.");

        if (r.PossuiTroca && string.IsNullOrWhiteSpace(r.DadosTroca))
            return Falha(TipoErro.Invalido, "Informe os dados do veiculo de troca.");

        if (r.ValorEntrada is not null && r.ValorEntrada > veiculo.Preco)
            return Falha(TipoErro.Invalido, "Valor de entrada nao pode exceder o preco do veiculo.");

        var email = r.EmailCliente.Trim().ToLowerInvariant();
        var telefone = r.TelefoneCliente.Trim();
        var cliente = await _repository.ObterClienteAsync(email, telefone, ct)
            ?? new Cliente { Nome = r.NomeCliente.Trim(), Email = email, Telefone = telefone };

        var agora = DateTimeOffset.UtcNow;
        var contato = new Contato
        {
            Cliente = cliente,
            ClienteId = cliente.Id == 0 ? null : cliente.Id,
            VeiculoId = veiculo.Id,
            LojaId = lojaId,
            Tipo = TipoContatoInicioCompra,
            Mensagem = MontarMensagem(r, veiculo, forma.Nome),
            Data = agora
        };

        await _repository.AdicionarContatoAsync(contato, ct);
        await _repository.SalvarAsync(ct);

        return Resultado<PropostaResponse>.Ok(new PropostaResponse(
            GerarProtocolo(contato.Id, agora), contato.Id, veiculo.Id, lojaId, cliente.Nome, cliente.Email,
            cliente.Telefone, forma.Nome, r.PossuiTroca, r.ValorEntrada, agora,
            "Proposta recebida. A equipe comercial da loja entrara em contato para concluir a negociacao."));
    }

    public static string GerarProtocolo(int id, DateTimeOffset data) =>
        $"PROP-{data.Year}-{id.ToString("D6", CultureInfo.InvariantCulture)}";

    private static string MontarMensagem(SolicitarPropostaRequest r, Veiculo v, string forma) =>
        string.Join('\n', new[]
        {
            "Inicio de compra (proposta comercial)",
            $"Veiculo: {v.Marca} {v.Modelo} {v.Ano} (id {v.Id})",
            $"Preco anunciado: {v.Preco.ToString("0.00", CultureInfo.InvariantCulture)}",
            $"Forma de pagamento: {forma}",
            $"Valor de entrada: {(r.ValorEntrada is null ? "nao informado" : r.ValorEntrada.Value.ToString("0.00", CultureInfo.InvariantCulture))}",
            $"Possui troca: {(r.PossuiTroca ? "Sim" : "Nao")}",
            r.PossuiTroca ? $"Dados da troca: {r.DadosTroca!.Trim()}" : null
        }.Where(l => l is not null)!);

    private static Resultado<PropostaResponse> Falha(TipoErro erro, string msg) => Resultado<PropostaResponse>.Falha(erro, msg);
}
