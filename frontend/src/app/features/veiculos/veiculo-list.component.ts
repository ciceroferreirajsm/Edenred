import { Component, DestroyRef, ElementRef, OnInit, inject, signal, viewChild } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { Subject, debounceTime, distinctUntilChanged } from 'rxjs';
import { PagedResult, Pessoa, Veiculo } from '../../core/models/models';
import { PessoaService } from '../../core/services/pessoa.service';
import { ToastService } from '../../core/services/toast.service';
import { VeiculoService } from '../../core/services/veiculo.service';
import { PaginatorComponent } from '../../shared/components/paginator.component';
import { CpfPipe } from '../../shared/pipes/format.pipes';
import { mensagemDeErro } from '../../shared/utils/form-errors';

@Component({
  selector: 'app-veiculo-list',
  imports: [RouterLink, FormsModule, PaginatorComponent, CpfPipe],
  templateUrl: './veiculo-list.component.html',
})
export class VeiculoListComponent implements OnInit {
  private readonly service = inject(VeiculoService);
  private readonly pessoaService = inject(PessoaService);
  private readonly toast = inject(ToastService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly busca$ = new Subject<string>();

  private readonly dialog = viewChild.required<ElementRef<HTMLDialogElement>>('dialogTransferir');

  protected readonly busca = signal('');
  protected readonly pagina = signal(1);
  protected readonly carregando = signal(false);
  protected readonly resultado = signal<PagedResult<Veiculo> | null>(null);

  // Estado do diálogo de vinculação/transferência
  protected readonly veiculoSelecionado = signal<Veiculo | null>(null);
  protected readonly pessoas = signal<Pessoa[]>([]);
  protected readonly novaPessoaId = signal('');
  protected readonly transferindo = signal(false);

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

  protected excluir(v: Veiculo): void {
    if (!confirm(`Excluir o veículo ${v.placa}?`)) return;

    this.service.excluir(v.id).subscribe({
      next: () => {
        this.toast.sucesso('Veículo excluído.');
        if (this.resultado()?.itens.length === 1 && this.pagina() > 1) this.pagina.update((p) => p - 1);
        this.carregar();
      },
      error: (e) => this.toast.erro(mensagemDeErro(e)),
    });
  }

  protected abrirTransferencia(v: Veiculo): void {
    this.veiculoSelecionado.set(v);
    this.novaPessoaId.set('');
    this.dialog().nativeElement.showModal();

    this.pessoaService.listar('', 1, 100).subscribe({
      next: (r) => this.pessoas.set(r.itens.filter((p) => p.id !== v.pessoaId)),
      error: (e) => this.toast.erro(mensagemDeErro(e)),
    });
  }

  protected fecharTransferencia(): void {
    this.dialog().nativeElement.close();
    this.veiculoSelecionado.set(null);
  }

  protected confirmarTransferencia(): void {
    const v = this.veiculoSelecionado();
    const pessoaId = this.novaPessoaId();
    if (!v || !pessoaId) return;

    this.transferindo.set(true);
    this.service.vincularPessoa(v.id, pessoaId).subscribe({
      next: (atualizado) => {
        this.transferindo.set(false);
        this.toast.sucesso(`Veículo ${atualizado.placa} vinculado a ${atualizado.pessoaNome}.`);
        this.fecharTransferencia();
        this.carregar();
      },
      error: (e) => {
        this.transferindo.set(false);
        this.toast.erro(mensagemDeErro(e));
      },
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
