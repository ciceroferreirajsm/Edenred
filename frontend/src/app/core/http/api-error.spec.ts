import { HttpErrorResponse } from '@angular/common/http';
import { toApiError } from './api-error';

describe('toApiError', () => {
  it('converte ValidationProblemDetails em erros por campo', () => {
    const erro = toApiError(
      new HttpErrorResponse({
        status: 400,
        error: {
          title: 'Dados inválidos',
          detail: 'Um ou mais campos possuem erros de validação.',
          errors: { email: ['E-mail em formato inválido.'], Nome: ['Nome é obrigatório.'] },
        },
      }),
    );

    expect(erro.status).toBe(400);
    expect(erro.isValidation).toBe(true);
    expect(erro.fieldErrors).toEqual({
      email: ['E-mail em formato inválido.'],
      nome: ['Nome é obrigatório.'],
    });
  });

  it('usa o detail do ProblemDetails como mensagem (ex.: 409)', () => {
    const erro = toApiError(
      new HttpErrorResponse({ status: 409, error: { title: 'Conflito', detail: 'CPF já cadastrado.' } }),
    );

    expect(erro.message).toBe('CPF já cadastrado.');
    expect(erro.isValidation).toBe(false);
  });

  it('retorna mensagem amigável quando a API está fora do ar', () => {
    const erro = toApiError(new HttpErrorResponse({ status: 0 }));
    expect(erro.message).toContain('Não foi possível conectar');
  });

  it('remove o prefixo "$." de erros de binding JSON', () => {
    const erro = toApiError(
      new HttpErrorResponse({ status: 400, error: { errors: { '$.dataNascimento': ['inválido'] } } }),
    );
    expect(Object.keys(erro.fieldErrors)).toEqual(['dataNascimento']);
  });
});
