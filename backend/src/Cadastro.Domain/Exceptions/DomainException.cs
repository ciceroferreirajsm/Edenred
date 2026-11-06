namespace Cadastro.Domain.Exceptions;

/// <summary>
/// Violação de invariante de domínio (estado inválido de uma entidade).
/// </summary>
public class DomainException(string message) : Exception(message);
