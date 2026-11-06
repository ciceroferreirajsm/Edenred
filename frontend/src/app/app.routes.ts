import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'pessoas' },
  {
    path: 'pessoas',
    title: 'Pessoas',
    loadComponent: () => import('./features/pessoas/pessoa-list.component').then((m) => m.PessoaListComponent),
  },
  {
    path: 'pessoas/nova',
    title: 'Nova pessoa',
    loadComponent: () => import('./features/pessoas/pessoa-form.component').then((m) => m.PessoaFormComponent),
  },
  {
    path: 'pessoas/:id',
    title: 'Pessoa',
    loadComponent: () => import('./features/pessoas/pessoa-detail.component').then((m) => m.PessoaDetailComponent),
  },
  {
    path: 'pessoas/:id/editar',
    title: 'Editar pessoa',
    loadComponent: () => import('./features/pessoas/pessoa-form.component').then((m) => m.PessoaFormComponent),
  },
  {
    path: 'veiculos',
    title: 'Veículos',
    loadComponent: () => import('./features/veiculos/veiculo-list.component').then((m) => m.VeiculoListComponent),
  },
  {
    path: 'veiculos/novo',
    title: 'Novo veículo',
    loadComponent: () => import('./features/veiculos/veiculo-form.component').then((m) => m.VeiculoFormComponent),
  },
  {
    path: 'veiculos/:id/editar',
    title: 'Editar veículo',
    loadComponent: () => import('./features/veiculos/veiculo-form.component').then((m) => m.VeiculoFormComponent),
  },
  { path: '**', redirectTo: 'pessoas' },
];
