import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { API_URL } from '../api.config';
import { PagedResult, Pessoa, PessoaRequest, Veiculo } from '../models/models';

@Injectable({ providedIn: 'root' })
export class PessoaService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${inject(API_URL)}/pessoas`;

  listar(busca = '', pagina = 1, tamanhoPagina = 10): Observable<PagedResult<Pessoa>> {
    let params = new HttpParams().set('pagina', pagina).set('tamanhoPagina', tamanhoPagina);
    if (busca.trim()) params = params.set('busca', busca.trim());
    return this.http.get<PagedResult<Pessoa>>(this.baseUrl, { params });
  }

  obter(id: string): Observable<Pessoa> {
    return this.http.get<Pessoa>(`${this.baseUrl}/${id}`);
  }

  listarVeiculos(id: string): Observable<PagedResult<Veiculo>> {
    return this.http.get<PagedResult<Veiculo>>(`${this.baseUrl}/${id}/veiculos`);
  }

  criar(request: PessoaRequest): Observable<Pessoa> {
    return this.http.post<Pessoa>(this.baseUrl, request);
  }

  atualizar(id: string, request: PessoaRequest): Observable<Pessoa> {
    return this.http.put<Pessoa>(`${this.baseUrl}/${id}`, request);
  }

  excluir(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
