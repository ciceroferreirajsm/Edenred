import { Injectable, signal } from '@angular/core';

export type ToastTipo = 'sucesso' | 'erro' | 'info';

export interface Toast {
  id: number;
  tipo: ToastTipo;
  mensagem: string;
}

@Injectable({ providedIn: 'root' })
export class ToastService {
  private seq = 0;
  private readonly _toasts = signal<Toast[]>([]);
  readonly toasts = this._toasts.asReadonly();

  sucesso(mensagem: string): void {
    this.mostrar('sucesso', mensagem);
  }

  erro(mensagem: string): void {
    this.mostrar('erro', mensagem, 7000);
  }

  info(mensagem: string): void {
    this.mostrar('info', mensagem);
  }

  fechar(id: number): void {
    this._toasts.update((lista) => lista.filter((t) => t.id !== id));
  }

  private mostrar(tipo: ToastTipo, mensagem: string, duracaoMs = 4000): void {
    const id = ++this.seq;
    this._toasts.update((lista) => [...lista, { id, tipo, mensagem }]);
    setTimeout(() => this.fechar(id), duracaoMs);
  }
}
