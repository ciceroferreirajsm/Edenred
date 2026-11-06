import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { errorInterceptor } from '../../core/http/error.interceptor';
import { PessoaFormComponent } from './pessoa-form.component';

describe('PessoaFormComponent', () => {
  let http: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PessoaFormComponent],
      providers: [
        provideRouter([]),
        provideHttpClient(withInterceptors([errorInterceptor])),
        provideHttpClientTesting(),
      ],
    }).compileComponents();
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  function criar() {
    const fixture = TestBed.createComponent(PessoaFormComponent);
    fixture.detectChanges();
    const el = fixture.nativeElement as HTMLElement;
    const preencher = (nome: string, valor: string) => {
      const input = el.querySelector<HTMLInputElement>(`[formControlName="${nome}"]`)!;
      input.value = valor;
      input.dispatchEvent(new Event('input'));
    };
    return { fixture, el, preencher };
  }

  it('não envia o formulário e exibe erros quando inválido', async () => {
    const { fixture, el } = criar();

    el.querySelector('form')!.dispatchEvent(new Event('submit'));
    await fixture.whenStable();

    const erros = Array.from(el.querySelectorAll('.field-error')).map((e) => e.textContent?.trim());
    expect(erros).toContain('Campo obrigatório.');
    http.expectNone('/api/pessoas');
  });

  it('aplica máscara de CPF ao digitar e valida dígitos verificadores', async () => {
    const { fixture, el, preencher } = criar();

    preencher('cpf', '52998224726');
    el.querySelector<HTMLInputElement>('[formControlName="cpf"]')!.dispatchEvent(new Event('blur'));
    await fixture.whenStable();

    expect(el.querySelector<HTMLInputElement>('[formControlName="cpf"]')!.value).toBe('529.982.247-26');
    expect(el.textContent).toContain('CPF inválido.');
  });

  it('envia dados normalizados e navega para o detalhe ao salvar', async () => {
    const { fixture, el, preencher } = criar();
    const navigate = vi.spyOn(TestBed.inject(Router), 'navigate').mockResolvedValue(true);

    preencher('nome', '  Maria Silva ');
    preencher('cpf', '52998224725');
    preencher('dataNascimento', '1990-05-20');
    preencher('email', 'maria@email.com');
    preencher('telefone', '11987654321');
    el.querySelector('form')!.dispatchEvent(new Event('submit'));

    const req = http.expectOne('/api/pessoas');
    expect(req.request.body).toEqual({
      nome: 'Maria Silva',
      cpf: '52998224725',
      dataNascimento: '1990-05-20',
      email: 'maria@email.com',
      telefone: '11987654321',
    });
    req.flush({ id: 'novo-id' });
    await fixture.whenStable();

    expect(navigate).toHaveBeenCalledWith(['/pessoas', 'novo-id']);
  });

  it('mostra no campo correspondente o erro de validação devolvido pela API', async () => {
    const { fixture, el, preencher } = criar();

    preencher('nome', 'Maria');
    preencher('cpf', '52998224725');
    preencher('dataNascimento', '1990-05-20');
    preencher('email', 'maria@email.com');
    el.querySelector('form')!.dispatchEvent(new Event('submit'));

    http.expectOne('/api/pessoas').flush(
      { title: 'Dados inválidos', errors: { email: ['E-mail já utilizado em outro sistema.'] } },
      { status: 400, statusText: 'Bad Request' },
    );
    await fixture.whenStable();

    expect(el.textContent).toContain('E-mail já utilizado em outro sistema.');
  });
});
