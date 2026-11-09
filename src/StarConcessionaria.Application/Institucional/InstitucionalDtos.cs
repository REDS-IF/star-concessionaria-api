namespace StarConcessionaria.Application.Institucional;

public record FormaPagamentoDetalheResponse(int Id, string Nome, string Descricao, bool AceitaOnline);
public record FaqResponse(int Id, string Pergunta, string Resposta);
