import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';

import { AuthService } from '../core/services/auth.service';
import { DashboardService } from './services/dashboard.service';
import { DashboardStats, AlertItem, QuickAccessItem } from './models/dashboard.model';
import { StatCardComponent } from './components/stat-card/stat-card.component';
import { QuickAccessCardComponent } from './components/quick-access-card/quick-access-card.component';
import { AlertsPanelComponent } from './components/alerts-panel/alerts-panel.component';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatIconModule,
    MatButtonModule,
    MatTooltipModule,
    StatCardComponent,
    QuickAccessCardComponent,
    AlertsPanelComponent
  ],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class DashboardComponent implements OnInit {
  private authService = inject(AuthService);
  private dashboardService = inject(DashboardService);

  // Estado
  readonly isLoading = signal(true);
  readonly stats = signal<DashboardStats | null>(null);
  readonly alerts = signal<AlertItem[]>([]);
  readonly error = signal<string | null>(null);

  // Computed
  readonly userName = computed(() => {
    const user = this.authService.currentUser();
    return user?.nombreCompleto || user?.nombreUsuario || 'Usuario';
  });

  readonly greeting = computed(() => {
    const hour = new Date().getHours();
    if (hour < 12) return 'Buenos dias';
    if (hour < 18) return 'Buenas tardes';
    return 'Buenas noches';
  });

  // Quick access items
  readonly quickAccessItems = computed<QuickAccessItem[]>(() => {
    const s = this.stats();
    return [
      {
        title: 'Usuarios',
        description: 'Gestiona los usuarios del sistema',
        icon: 'people',
        route: '/seguridad/usuarios',
        color: 'cyan',
        stats: s ? [
          { label: 'Total', value: s.seguridad.usuarios.total },
          { label: 'Activos', value: s.seguridad.usuarios.activos }
        ] : []
      },
      {
        title: 'Perfiles',
        description: 'Configura perfiles de acceso',
        icon: 'folder_shared',
        route: '/seguridad/perfiles',
        color: 'purple',
        stats: s ? [
          { label: 'Total', value: s.seguridad.perfiles.total }
        ] : []
      },
      {
        title: 'Roles',
        description: 'Define roles y permisos',
        icon: 'badge',
        route: '/seguridad/roles',
        color: 'green',
        stats: s ? [
          { label: 'Total', value: s.seguridad.roles.total }
        ] : []
      },
      {
        title: 'Documentos',
        description: 'Administra archivos del sistema',
        icon: 'description',
        route: '/documento',
        color: 'blue',
        stats: s ? [
          { label: 'Total', value: s.documentos.total }
        ] : []
      }
    ];
  });

  ngOnInit(): void {
    this.loadDashboardData();
  }

  loadDashboardData(): void {
    this.isLoading.set(true);
    this.error.set(null);

    this.dashboardService.getDashboardStats().subscribe({
      next: (data) => {
        this.stats.set(data);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('[Dashboard] Error loading stats:', err);
        this.error.set('Error al cargar los datos del dashboard');
        this.isLoading.set(false);
      }
    });

    this.dashboardService.getAlerts().subscribe({
      next: (alerts) => this.alerts.set(alerts),
      error: (err) => console.error('[Dashboard] Error loading alerts:', err)
    });
  }

  refresh(): void {
    this.loadDashboardData();
  }
}
