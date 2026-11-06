namespace Cadastro.Application.Common;

/// <summary>Recurso solicitado não existe (HTTP 404).</summary>
public class NotFoundException(string message) : Exception(message);

/// <summary>Operação conflita com o estado atual dos dados, ex.: CPF/placa duplicados (HTTP 409).</summary>
public class ConflictException(string message) : Exception(message);
