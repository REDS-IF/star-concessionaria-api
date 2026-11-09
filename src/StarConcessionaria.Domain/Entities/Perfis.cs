namespace StarConcessionaria.Domain.Entities;

/// <summary>Perfis RBAC. No banco sao gravados em minusculas; nos claims usam-se estes nomes canonicos.</summary>
public static class Perfis
{
    public const string Administrador = "Administrador";
    public const string Comercial = "Comercial";
    public const string Logistica = "Logistica";

    public static string? Normalizar(string? perfilBanco) => perfilBanco?.Trim().ToLowerInvariant() switch
    {
        "administrador" => Administrador,
        "comercial" => Comercial,
        "logistica" => Logistica,
        _ => null
    };
}
