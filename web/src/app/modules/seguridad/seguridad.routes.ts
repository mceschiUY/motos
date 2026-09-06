import { Routes } from '@angular/router';

export const SEGURIDAD_ROUTES: Routes = [
  {
    path: '',
    redirectTo: 'usuarios',
    pathMatch: 'full'
  },
  {
    path: 'usuarios',
    loadComponent: () => import('./components/usuarios/usuario-list/usuario-list.component')
      .then(m => m.UsuarioListComponent),
    title: 'Usuarios'
  },
  {
    path: 'perfiles',
    loadComponent: () => import('./components/perfiles/perfil-list/perfil-list.component')
      .then(m => m.PerfilListComponent),
    title: 'Perfiles'
  },
  {
    path: 'roles',
    loadComponent: () => import('./components/roles/rol-list/rol-list.component')
      .then(m => m.RolListComponent),
    title: 'Roles'
  },
  {
    path: 'capabilities',
    loadComponent: () => import('./components/capabilities/capability-list/capability-list.component')
      .then(m => m.CapabilityListComponent),
    title: 'Capabilities'
  }
];
