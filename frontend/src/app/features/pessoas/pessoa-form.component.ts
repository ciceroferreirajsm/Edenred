import { Component, OnInit, inject, input, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { PessoaRequest } from '../../core/models/models';
import { PessoaService } from '../../core/services/pessoa.service';
import { ToastService } from '../../core/services/toast.service';
import { FieldErrorComponent } from '../../shared/components/field-error.component';
import { MaskDirective } from '../../shared/directives/mask.directive';
import { formatarCpf, formatarTelefone, somenteDigitos } from '../../shared/utils/formatters';
import { aplicarErrosDoServidor, mensagemDeErro } from '../../shared/utils/form-errors';
import { AppValidators } from '../../shared/validators/validators';

@Component({
  selector: 'app-pessoa-form',
  imports: [ReactiveFormsModule, RouterLink, FieldErrorComponent, MaskDirective],
  templateUrl: './pessoa-form.component.html',
})
export class PessoaFormComponent implements OnInit {
  /** Preenchido pela rota `pessoas/:id/editar` (withComponentInputBinding). */
  readonly id = input<string>();

  private readonly fb = inject(FormBuilder).nonNullable;
  private readonly service = inject(PessoaService);
  private readonly toast = inject(ToastService);
  private readonly router = inject(Router);

  protected readonly salvando = signal(false);
  protected readonly carregando = signal(false);

  protected readonly form = this.fb.group({
    nome: ['', [Validators.required, Validators.maxLength(150)]],
    cpf: ['', [Validators.required, AppValidators.cpf]],
    dataNascimento: ['', [Validators.required, AppValidators.dataNaoFutura()]],
    email: ['', [Validators.required, Validators.maxLength(200), AppValidators.email]],
    telefone: ['', [AppValidators.telefone]],
  });

  get editando(): boolean {
    return !!this.id();
  }

  ngOnInit(): void {
    const id = this.id();
    if (!id) return;

    this.carregando.set(true);
    this.service.obter(id).subscribe({
      next: (p) => {
        this.form.setValue({
          nome: p.nome,
          cpf: formatarCpf(p.cpf),
          dataNascimento: p.dataNascimento,
          email: p.email,
          telefone: formatarTelefone(p.telefone),
        });
        this.carregando.set(false);
      },
      error: (e) => {
        this.toast.erro(mensagemDeErro(e));
        this.router.navigate(['/pessoas']);
      },
    });
  }

  protected salvar(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const v = this.form.getRawValue();
    const request: PessoaRequest = {
      nome: v.nome.trim(),
      cpf: somenteDigitos(v.cpf),
      dataNascimento: v.dataNascimento,
      email: v.email.trim(),
      telefone: somenteDigitos(v.telefone) || null,
    };

    const id = this.id();
    const operacao = id ? this.service.atualizar(id, request) : this.service.criar(request);

    this.salvando.set(true);
    operacao.subscribe({
      next: (pessoa) => {
        this.salvando.set(false);
        this.toast.sucesso(id ? 'Pessoa atualizada.' : 'Pessoa cadastrada.');
        this.router.navigate(['/pessoas', pessoa.id]);
      },
      error: (e) => {
        this.salvando.set(false);
        if (!aplicarErrosDoServidor(this.form, e)) this.toast.erro(mensagemDeErro(e));
      },
    });
  }
}
