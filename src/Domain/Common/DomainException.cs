namespace StarConcessionaria.Domain.Common;

/// <summary>
/// Exceção base para violações de invariantes e regras do Domínio.
/// </summary>
public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message) { }
}
