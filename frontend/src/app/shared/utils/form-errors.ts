import { FormGroup } from '@angular/forms';
import { ApiError } from '../../core/http/api-error';

/**
 * Aplica os erros de validação devolvidos pela API (chaves em camelCase)
 * nos controles correspondentes do formulário, sob a chave `server`.
 * Retorna true se algum erro foi associado a um campo.
 */
export function aplicarErrosDoServidor(form: FormGroup, erro: unknown): boolean {
  if (!(erro instanceof ApiError) || !erro.isValidation) return false;

  let aplicou = false;
  for (const [campo, mensagens] of Object.entries(erro.fieldErrors)) {
    const control = form.get(campo);
    if (control) {
      control.setErrors({ ...(control.errors ?? {}), server: mensagens.join(' ') });
      control.markAsTouched();
      aplicou = true;
    }
  }
  return aplicou;
}

export function mensagemDeErro(erro: unknown): string {
  return erro instanceof Error ? erro.message : 'Erro inesperado.';
}
