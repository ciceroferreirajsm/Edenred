import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { VeiculoService } from './veiculo.service';

describe('VeiculoService', () => {
  let service: VeiculoService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(VeiculoService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('listar filtra por pessoa quando informado', () => {
    service.listar('', 1, 10, 'pessoa-1').subscribe();

    const req = http.expectOne((r) => r.url === '/api/veiculos');
    expect(req.request.params.get('pessoaId')).toBe('pessoa-1');
    req.flush({ itens: [], pagina: 1, tamanhoPagina: 10, totalItens: 0, totalPaginas: 0 });
  });

  it('vincularPessoa faz PATCH em /veiculos/{id}/pessoa', () => {
    service.vincularPessoa('v1', 'p2').subscribe();

    const req = http.expectOne('/api/veiculos/v1/pessoa');
    expect(req.request.method).toBe('PATCH');
    expect(req.request.body).toEqual({ pessoaId: 'p2' });
    req.flush({});
  });

  it('excluir faz DELETE', () => {
    service.excluir('v1').subscribe();
    const req = http.expectOne('/api/veiculos/v1');
    expect(req.request.method).toBe('DELETE');
    req.flush(null, { status: 204, statusText: 'No Content' });
  });
});
