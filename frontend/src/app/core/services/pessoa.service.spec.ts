import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { ApiError } from '../http/api-error';
import { errorInterceptor } from '../http/error.interceptor';
import { PessoaService } from './pessoa.service';

describe('PessoaService', () => {
  let service: PessoaService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(withInterceptors([errorInterceptor])), provideHttpClientTesting()],
    });
    service = TestBed.inject(PessoaService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('listar envia busca e paginação como query string', () => {
    service.listar('  maria ', 2, 5).subscribe();

    const req = http.expectOne((r) => r.url === '/api/pessoas');
    expect(req.request.method).toBe('GET');
    expect(req.request.params.get('busca')).toBe('maria');
    expect(req.request.params.get('pagina')).toBe('2');
    expect(req.request.params.get('tamanhoPagina')).toBe('5');
    req.flush({ itens: [], pagina: 2, tamanhoPagina: 5, totalItens: 0, totalPaginas: 0 });
  });

  it('listar omite busca vazia', () => {
    service.listar().subscribe();
    const req = http.expectOne((r) => r.url === '/api/pessoas');
    expect(req.request.params.has('busca')).toBe(false);
    req.flush({ itens: [], pagina: 1, tamanhoPagina: 10, totalItens: 0, totalPaginas: 0 });
  });

  it('criar faz POST com o corpo informado', () => {
    const body = { nome: 'Maria', cpf: '52998224725', dataNascimento: '1990-05-20', email: 'm@e.com', telefone: null };
    service.criar(body).subscribe();

    const req = http.expectOne('/api/pessoas');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(body);
    req.flush({ id: '1', ...body });
  });

  it('erros HTTP chegam aos componentes como ApiError (via interceptor)', () => {
    let erro: unknown;
    service.excluir('abc').subscribe({ error: (e) => (erro = e) });

    http
      .expectOne('/api/pessoas/abc')
      .flush({ title: 'Conflito', detail: 'Há veículos vinculados.' }, { status: 409, statusText: 'Conflict' });

    expect(erro).toBeInstanceOf(ApiError);
    expect((erro as ApiError).status).toBe(409);
    expect((erro as ApiError).message).toBe('Há veículos vinculados.');
  });
});
