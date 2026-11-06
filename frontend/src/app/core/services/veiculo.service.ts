import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { API_URL } from '../api.config';
import { PagedResult, Veiculo, VeiculoRequest } from '../models/models';

@Injectable({ providedIn: 'root' })
export class VeiculoService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${inject(API_URL)}/veiculos`;

  listar(busca = '', pagina = 1, tamanhoPagina = 10, pessoaId?: string): Observable<PagedResult<Veiculo>> {
    let params = new HttpParams().set('pagina', pagina).set('tamanhoPagina', tamanhoPagina);
    if (busca.trim()) params = params.set('busca', busca.trim());
    if (pessoaId) params = params.set('pessoaId', pessoaId);
    return this.http.get<PagedResult<Veiculo>>(this.baseUrl, { params });
  }

  obter(id: string): Observable<Veiculo> {
    return this.http.get<Veiculo>(`${this.baseUrl}/${id}`);
  }

  criar(request: VeiculoRequest): Observable<Veiculo> {
    return this.http.post<Veiculo>(this.baseUrl, request);
  }

  atualizar(id: string, request: VeiculoRequest): Observable<Veiculo> {
    return this.http.put<Veiculo>(`${this.baseUrl}/${id}`, request);
  }

  /** Vincula/transfere o veículo para outra pessoa. */
  vincularPessoa(id: string, pessoaId: string): Observable<Veiculo> {
    return this.http.patch<Veiculo>(`${this.baseUrl}/${id}/pessoa`, { pessoaId });
  }

  excluir(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
