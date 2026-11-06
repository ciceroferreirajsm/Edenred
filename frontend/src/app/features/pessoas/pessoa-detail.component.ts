import { Component, OnInit, inject, input, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import { Pessoa, Veiculo } from '../../core/models/models';
import { PessoaService } from '../../core/services/pessoa.service';
import { ToastService } from '../../core/services/toast.service';
import { VeiculoService } from '../../core/services/veiculo.service';
import { CpfPipe, DataBrPipe, TelefonePipe } from '../../shared/pipes/format.pipes';
import { mensagemDeErro } from '../../shared/utils/form-errors';

@Component({
  selector: 'app-pessoa-detail',
  imports: [RouterLink, CpfPipe, TelefonePipe, DataBrPipe],
  templateUrl: './pessoa-detail.component.html',
})
export class PessoaDetailComponent implements OnInit {
  readonly id = input.required<string>();

  private readonly pessoaService = inject(PessoaService);
  private readonly veiculoService = inject(VeiculoService);
  private readonly toast = inject(ToastService);
  private readonly router = inject(Router);

  protected readonly pessoa = signal<Pessoa | null>(null);
  protected readonly veiculos = signal<Veiculo[]>([]);

  ngOnInit(): void {
    this.carregar();
  }

  protected excluirPessoa(): void {
    const p = this.pessoa();
    if (!p) return;
    if (this.veiculos().length > 0) {
      this.toast.erro('Exclua ou transfira os veículos desta pessoa antes de excluí-la.');
      return;
    }
    if (!confirm(`Excluir ${p.nome}?`)) return;

    this.pessoaService.excluir(p.id).subscribe({
      next: () => {
        this.toast.sucesso('Pessoa excluída.');
        this.router.navigate(['/pessoas']);
      },
      error: (e) => this.toast.erro(mensagemDeErro(e)),
    });
  }

  protected excluirVeiculo(v: Veiculo): void {
    if (!confirm(`Excluir o veículo ${v.placa}?`)) return;

    this.veiculoService.excluir(v.id).subscribe({
      next: () => {
        this.toast.sucesso('Veículo excluído.');
        this.carregar();
      },
      error: (e) => this.toast.erro(mensagemDeErro(e)),
    });
  }

  private carregar(): void {
    forkJoin({
      pessoa: this.pessoaService.obter(this.id()),
      veiculos: this.pessoaService.listarVeiculos(this.id()),
    }).subscribe({
      next: ({ pessoa, veiculos }) => {
        this.pessoa.set(pessoa);
        this.veiculos.set(veiculos.itens);
      },
      error: (e) => {
        this.toast.erro(mensagemDeErro(e));
        this.router.navigate(['/pessoas']);
      },
    });
  }
}
