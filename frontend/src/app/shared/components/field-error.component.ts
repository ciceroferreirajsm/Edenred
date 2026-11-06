import { Component, computed, input } from '@angular/core';
import { toObservable, toSignal } from '@angular/core/rxjs-interop';
import { AbstractControl } from '@angular/forms';
import { startWith, switchMap } from 'rxjs';

const MENSAGENS: Record<string, (valor: any) => string> = {
  required: () => 'Campo obrigatório.',
  maxlength: (e) => `Máximo de ${e.requiredLength} caracteres.`,
  min: (e) => `Valor mínimo: ${e.min}.`,
  max: (e) => `Valor máximo: ${e.max}.`,
  email: () => 'E-mail em formato inválido.',
  cpf: () => 'CPF inválido.',
  placa: () => 'Placa inválida. Use ABC1234 ou ABC1D23 (Mercosul).',
  telefone: () => 'Informe DDD + número (10 ou 11 dígitos).',
  dataFutura: () => 'Data não pode estar no futuro.',
  anoFuturo: (e) => `Ano de fabricação não pode ser maior que o ano atual (${e.max}).`,
  server: (e) => String(e),
};

/**
 * Exibe a primeira mensagem de erro de um controle após ele ser tocado.
 * Reage a `control.events` (valor, status, touched), então funciona com OnPush/zoneless.
 */
@Component({
  selector: 'app-field-error',
  template: `
    @if (mensagem(); as msg) {
      <small class="field-error" role="alert">{{ msg }}</small>
    }
  `,
})
export class FieldErrorComponent {
  readonly control = input.required<AbstractControl>();

  private readonly evento = toSignal(
    toObservable(this.control).pipe(switchMap((c) => c.events.pipe(startWith(null)))),
  );

  protected readonly mensagem = computed(() => {
    this.evento();
    const c = this.control();
    if (!c.errors || !(c.touched || c.dirty)) return null;

    const [chave, valor] = Object.entries(c.errors)[0];
    return MENSAGENS[chave]?.(valor) ?? 'Valor inválido.';
  });
}
