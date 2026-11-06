import { Component, OnInit, inject, input, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { Pessoa, VeiculoRequest } from '../../core/models/models';
import { PessoaService } from '../../core/services/pessoa.service';
import { ToastService } from '../../core/services/toast.service';
import { VeiculoService } from '../../core/services/veiculo.service';
import { FieldErrorComponent } from '../../shared/components/field-error.component';
import { MaskDirective } from '../../shared/directives/mask.directive';
import { CpfPipe } from '../../shared/pipes/format.pipes';
import { formatarPlaca } from '../../shared/utils/formatters';
import { aplicarErrosDoServidor, mensagemDeErro } from '../../shared/utils/form-errors';
import { AppValidators } from '../../shared/validators/validators';

/** Sugestões apenas — marca e modelo continuam sendo texto livre (informados manualmente). */
const MARCAS_SUGERIDAS = [
  'Chevrolet', 'Fiat', 'Ford', 'Honda', 'Hyundai', 'Jeep', 'Nissan',
  'Peugeot', 'Renault', 'Toyota', 'Volkswagen', 'BYD', 'Yamaha',
];

@Component({
  selector: 'app-veiculo-form',
  imports: [ReactiveFormsModule, RouterLink, FieldErrorComponent, MaskDirective, CpfPipe],
  templateUrl: './veiculo-form.component.html',
})
export class VeiculoFormComponent implements OnInit {
  /** Rota `veiculos/:id/editar`. */
  readonly id = input<string>();
  /** Query param `?pessoaId=` para pré-selecionar o proprietário. */
  readonly pessoaId = input<string>();

  private readonly fb = inject(FormBuilder).nonNullable;
  private readonly service = inject(VeiculoService);
  private readonly pessoaService = inject(PessoaService);
  private readonly toast = inject(ToastService);
  private readonly router = inject(Router);

  protected readonly anoAtual = new Date().getFullYear();
  protected readonly marcas = MARCAS_SUGERIDAS;
  protected readonly pessoas = signal<Pessoa[]>([]);
  protected readonly salvando = signal(false);
  protected readonly carregando = signal(false);

  protected readonly form = this.fb.group({
    placa: ['', [Validators.required, AppValidators.placa]],
    anoFabricacao: [this.anoAtual, [Validators.required, Validators.min(1886), AppValidators.anoMaximoAtual()]],
    marca: ['', [Validators.required, Validators.maxLength(60)]],
    modelo: ['', [Validators.required, Validators.maxLength(80)]],
    pessoaId: ['', [Validators.required]],
  });

  get editando(): boolean {
    return !!this.id();
  }

  ngOnInit(): void {
    this.pessoaService.listar('', 1, 100).subscribe({
      next: (r) => this.pessoas.set(r.itens),
      error: (e) => this.toast.erro(mensagemDeErro(e)),
    });

    const preSelecionada = this.pessoaId();
    if (preSelecionada) this.form.controls.pessoaId.setValue(preSelecionada);

    const id = this.id();
    if (!id) return;

    this.carregando.set(true);
    this.service.obter(id).subscribe({
      next: (v) => {
        this.form.setValue({
          placa: v.placa,
          anoFabricacao: v.anoFabricacao,
          marca: v.marca,
          modelo: v.modelo,
          pessoaId: v.pessoaId,
        });
        this.carregando.set(false);
      },
      error: (e) => {
        this.toast.erro(mensagemDeErro(e));
        this.router.navigate(['/veiculos']);
      },
    });
  }

  protected salvar(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const v = this.form.getRawValue();
    const request: VeiculoRequest = {
      placa: formatarPlaca(v.placa),
      anoFabricacao: Number(v.anoFabricacao),
      marca: v.marca.trim(),
      modelo: v.modelo.trim(),
      pessoaId: v.pessoaId,
    };

    const id = this.id();
    const operacao = id ? this.service.atualizar(id, request) : this.service.criar(request);

    this.salvando.set(true);
    operacao.subscribe({
      next: (veiculo) => {
        this.salvando.set(false);
        this.toast.sucesso(id ? 'Veículo atualizado.' : 'Veículo cadastrado.');
        this.router.navigate(['/pessoas', veiculo.pessoaId]);
      },
      error: (e) => {
        this.salvando.set(false);
        if (!aplicarErrosDoServidor(this.form, e)) this.toast.erro(mensagemDeErro(e));
      },
    });
  }
}
