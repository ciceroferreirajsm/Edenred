import { Component, computed, input, output } from '@angular/core';

@Component({
  selector: 'app-paginator',
  template: `
    @if (totalItens() > 0) {
      <nav class="paginator" aria-label="Paginação">
        <span class="muted">{{ totalItens() }} registro(s) · página {{ pagina() }} de {{ totalPaginas() }}</span>
        <div class="paginator-actions">
          <button type="button" class="btn btn-sm" [disabled]="!temAnterior()" (click)="paginaChange.emit(pagina() - 1)">
            ‹ Anterior
          </button>
          <button type="button" class="btn btn-sm" [disabled]="!temProxima()" (click)="paginaChange.emit(pagina() + 1)">
            Próxima ›
          </button>
        </div>
      </nav>
    }
  `,
})
export class PaginatorComponent {
  readonly pagina = input.required<number>();
  readonly totalPaginas = input.required<number>();
  readonly totalItens = input.required<number>();
  readonly paginaChange = output<number>();

  protected readonly temAnterior = computed(() => this.pagina() > 1);
  protected readonly temProxima = computed(() => this.pagina() < this.totalPaginas());
}
