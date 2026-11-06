import { Component, DestroyRef, OnInit, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { Subject, debounceTime, distinctUntilChanged } from 'rxjs';
import { PagedResult, Pessoa } from '../../core/models/models';
import { PessoaService } from '../../core/services/pessoa.service';
import { ToastService } from '../../core/services/toast.service';
import { PaginatorComponent } from '../../shared/components/paginator.component';
import { CpfPipe, DataBrPipe, TelefonePipe } from '../../shared/pipes/format.pipes';
import { mensagemDeErro } from '../../shared/utils/form-errors';

@Component({
  selector: 'app-pessoa-list',
  imports: [RouterLink, PaginatorComponent, CpfPipe, TelefonePipe, DataBrPipe],
  templateUrl: './pessoa-list.component.html',
})
export class PessoaListComponent implements OnInit {
  private readonly service = inject(PessoaService);
  private readonly toast = inject(ToastService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly busca$ = new Subject<string>();

  protected readonly busca = signal('');
  protected readonly pagina = signal(1);
  protected readonly carregando = signal(false);
  protected readonly resultado = signal<PagedResult<Pessoa> | null>(null);

  ngOnInit(): void {
    this.busca$
      .pipe(debounceTime(350), distinctUntilChanged(), takeUntilDestroyed(this.destroyRef))
      .subscribe((termo) => {
        this.busca.set(termo);
        this.pagina.set(1);
        this.carregar();
      });

    this.carregar();
  }

  protected onBuscar(termo: string): void {
    this.busca$.next(termo);
  }

  protected irParaPagina(pagina: number): void {
    this.pagina.set(pagina);
    this.carregar();
  }

  protected excluir(pessoa: Pessoa): void {
    if (pessoa.quantidadeVeiculos > 0) {
      this.toast.erro(`${pessoa.nome} possui ${pessoa.quantidadeVeiculos} veículo(s). Exclua ou transfira antes.`);
      return;
    }
    if (!confirm(`Excluir ${pessoa.nome}?`)) return;

    this.service.excluir(pessoa.id).subscribe({
      next: () => {
        this.toast.sucesso('Pessoa excluída.');
        if (this.resultado()?.itens.length === 1 && this.pagina() > 1) this.pagina.update((p) => p - 1);
        this.carregar();
      },
      error: (e) => this.toast.erro(mensagemDeErro(e)),
    });
  }

  private carregar(): void {
    this.carregando.set(true);
    this.service.listar(this.busca(), this.pagina()).subscribe({
      next: (r) => {
        this.resultado.set(r);
        this.carregando.set(false);
      },
      error: (e) => {
        this.carregando.set(false);
        this.toast.erro(mensagemDeErro(e));
      },
    });
  }
}
