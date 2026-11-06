import { HttpErrorResponse } from '@angular/common/http';

/** Erro normalizado a partir das respostas Problem Details (RFC 9457) da API. */
export class ApiError extends Error {
  constructor(
    readonly status: number,
    message: string,
    readonly fieldErrors: Record<string, string[]> = {},
  ) {
    super(message);
    this.name = 'ApiError';
  }

  get isValidation(): boolean {
    return this.status === 400 && Object.keys(this.fieldErrors).length > 0;
  }
}

interface ProblemDetails {
  title?: string;
  detail?: string;
  errors?: Record<string, string[]>;
}

export function toApiError(error: HttpErrorResponse): ApiError {
  if (error.status === 0) {
    return new ApiError(0, 'Não foi possível conectar à API. Verifique se o backend está em execução.');
  }

  const body = (typeof error.error === 'object' && error.error !== null ? error.error : {}) as ProblemDetails;
  const fieldErrors = normalizarChaves(body.errors ?? {});

  const message =
    body.detail ??
    body.title ??
    (error.status >= 500 ? 'Erro interno no servidor.' : `Erro ${error.status} ao processar a requisição.`);

  return new ApiError(error.status, message, fieldErrors);
}

/** A API devolve chaves em camelCase; garantimos isso também para erros de binding (ex.: "$.dataNascimento"). */
function normalizarChaves(errors: Record<string, string[]>): Record<string, string[]> {
  return Object.fromEntries(
    Object.entries(errors).map(([key, value]) => {
      const limpo = key.replace(/^\$\.?/, '');
      return [limpo.charAt(0).toLowerCase() + limpo.slice(1), value];
    }),
  );
}
