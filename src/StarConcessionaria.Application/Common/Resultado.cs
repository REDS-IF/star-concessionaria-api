namespace StarConcessionaria.Application.Common;

public enum TipoErro { NaoEncontrado, Invalido, Conflito }

/// <summary>Resultado de caso de uso sem lancar excecoes para fluxo de negocio.</summary>
public class Resultado<T>
{
    public T? Valor { get; }
    public TipoErro? Erro { get; }
    public string? Mensagem { get; }
    public bool Sucesso => Erro is null;

    private Resultado(T? valor, TipoErro? erro, string? mensagem) { Valor = valor; Erro = erro; Mensagem = mensagem; }

    public static Resultado<T> Ok(T valor) => new(valor, null, null);
    public static Resultado<T> Falha(TipoErro erro, string mensagem) => new(default, erro, mensagem);
}
