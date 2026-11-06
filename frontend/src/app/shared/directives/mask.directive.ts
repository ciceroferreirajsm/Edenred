import { Directive, inject, input } from '@angular/core';
import { NgControl } from '@angular/forms';
import { formatarCpf, formatarPlaca, formatarTelefone } from '../utils/formatters';

export type TipoMascara = 'cpf' | 'telefone' | 'placa';

const FORMATADORES: Record<TipoMascara, (v: string) => string> = {
  cpf: formatarCpf,
  telefone: formatarTelefone,
  placa: formatarPlaca,
};

/** Aplica máscara simples ao digitar: `<input formControlName="cpf" appMask="cpf">`. */
@Directive({
  selector: '[appMask]',
  host: { '(input)': 'aoDigitar($event)' },
})
export class MaskDirective {
  readonly appMask = input.required<TipoMascara>();
  private readonly control = inject(NgControl, { self: true });

  aoDigitar(event: Event): void {
    const el = event.target as HTMLInputElement;
    const formatado = FORMATADORES[this.appMask()](el.value);
    if (formatado !== el.value) {
      this.control.control?.setValue(formatado);
    }
  }
}
