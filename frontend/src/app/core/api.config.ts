import { InjectionToken } from '@angular/core';

/**
 * URL base da API. Por padrão usa caminho relativo (`/api`):
 * - em desenvolvimento o `ng serve` encaminha para http://localhost:5000 via `proxy.conf.json`;
 * - no Docker o Nginx faz o mesmo encaminhamento para o container da API.
 */
export const API_URL = new InjectionToken<string>('API_URL', {
  providedIn: 'root',
  factory: () => '/api',
});
