import { Component, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule, RouterOutlet, NavigationEnd } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatRippleModule } from '@angular/material/core';
import { MatMenuModule } from '@angular/material/menu';
import { MatDividerModule } from '@angular/material/divider';
import { filter } from 'rxjs/operators';
import { GENERATED_MENU_ITEMS, GENERATED_MENU_GROUPS, GeneratedMenuItem, GeneratedMenuGroup } from '../modules/generated/generated-menu.registry';
import { AuthService } from '../core/services/auth.service';
import { ThemeService } from '../core/services/theme.service';
import { SiteConfigService } from '../core/services/site-config.service';
import { ActividadService } from '../core/services/actividad.service';
import { SiteFooterComponent } from './components/site-footer/site-footer.component';
import { AlchemyLensComponent } from './components/alchemy-lens/alchemy-lens.component';
import { HeaderSearchComponent } from '../shared/components/global-search/header-search.component';
import { AsistenteVozComponent } from '../shared/components/asistente-voz/asistente-voz.component';
import { environment } from '../../environments/environment';

interface SystemMenuItem {
  path: string;
  label: string;
  icon: string;
  children?: { path: string; label: string; icon: string }[];
}

@Component({
  selector: 'app-site-layout',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    RouterOutlet,
    MatIconModule,
    MatButtonModule,
    MatTooltipModule,
    MatRippleModule,
    MatMenuModule,
    MatDividerModule,
    SiteFooterComponent,
    AlchemyLensComponent,
    HeaderSearchComponent,
    AsistenteVozComponent
  ],
  templateUrl: './site-layout.component.html',
  styleUrl: './site-layout.component.scss'
})
export class SiteLayoutComponent {
  private router = inject(Router);
  authService = inject(AuthService);
  themeService = inject(ThemeService);
  siteConfig = inject(SiteConfigService);
  actividad = inject(ActividadService);

  // Herramientas del motor ZAS (mutación, lens, evolution, modo pantalla):
  // visibles solo fuera de producción — el cliente final no las ve en el build publicado.
  // Herramientas del motor (lens de diff, mutación, Evolution Hub, modo pantalla): OCULTAS por
  // defecto, también en `ng serve` (la demo corre así). Opt-in para desarrollo:
  // localStorage.setItem('zas.herramientas', '1') y recargar. (Etapa 0 del plan, 2026-09-07.)
  readonly herramientasZas = !environment.production && (() => {
    try { return localStorage.getItem('zas.herramientas') === '1'; } catch { return false; }
  })();

  sidebarCollapsed = signal(false);
  currentRoute = signal<string>('');
  currentPageTitle = signal<string>('Inicio');
  /** Cierre del rastro cuando la URL termina en un id ("Detalle"): el id crudo
   *  jamás se muestra en el cabezal (pedido Pablo 2026-09-06). */
  currentPageSufijo = signal<string>('');
  seguridadExpanded = signal(false);
  expandedGroups = signal<Set<string>>(new Set());

  // El registry trae un HOME_ITEM (path '') para el buscador global; acá se filtra
  // porque el sidebar ya tiene su "Inicio" fijo — sin esto aparecían dos Inicios.
  menuItems: GeneratedMenuItem[] = GENERATED_MENU_ITEMS.filter(i => i.path !== '');
  menuGroups: GeneratedMenuGroup[] = GENERATED_MENU_GROUPS;

  /** Rastro para el cabezal (casita → grupo → página): derivado de la ruta y el
   *  menú. Reemplaza al breadcrumb por vista, que ocupaba una fila entera. */
  readonly rastro = computed(() => {
    const ruta = this.currentRoute();
    if (!ruta) return [] as { label: string }[];
    const partes: { label: string }[] = [];
    const grupo = this.menuGroups.find(g => g.items.some(i => i.path === ruta));
    if (grupo) partes.push({ label: grupo.label });
    const item = this.menuItems.find(i => i.path === ruta)
      ?? this.seguridadMenu.children?.find(c => c.path === ruta);
    const label = item?.label ?? this.currentPageTitle();
    if (!partes.length || partes[partes.length - 1].label !== label) partes.push({ label });
    const sufijo = this.currentPageSufijo();
    if (sufijo) partes.push({ label: sufijo });
    return partes;
  });
  hasGroups = GENERATED_MENU_GROUPS.length > 0;
  hasComponents = GENERATED_MENU_ITEMS.length > 0;

  // Menu de Seguridad
  seguridadMenu: SystemMenuItem = {
    path: 'seguridad',
    label: 'Seguridad',
    icon: 'security',
    children: [
      { path: 'seguridad/usuarios', label: 'Usuarios', icon: 'people' },
      { path: 'seguridad/perfiles', label: 'Perfiles', icon: 'folder_shared' },
      { path: 'seguridad/roles', label: 'Roles', icon: 'badge' },
      { path: 'seguridad/capabilities', label: 'Capabilities', icon: 'vpn_key' }
    ]
  };

  // Menu item de Configuracion
  configuracionMenuItem: SystemMenuItem = {
    path: 'configuracion',
    label: 'Configuracion',
    icon: 'settings'
  };

  // Menu item de Auditoria
  auditoriaMenuItem: SystemMenuItem = {
    path: 'auditoria',
    label: 'Auditoria',
    icon: 'history'
  };

  // Menu item de Reportes
  reportesMenuItem: SystemMenuItem = {
    path: 'reportes',
    label: 'Reportes',
    icon: 'assessment'
  };

  constructor() {
    this.router.events.pipe(
      filter(event => event instanceof NavigationEnd)
    ).subscribe((event: any) => {
      this.updatePageTitle(event.url);
    });

    // Pulso del negocio: carga la actividad al entrar (enciende la campanita si hay novedades)
    this.actividad.cargar();
  }

  get userName(): string {
    const user = this.authService.currentUser();
    return user?.nombreCompleto || user?.nombreUsuario || 'Usuario';
  }

  get userProfile(): string {
    const user = this.authService.currentUser();
    return user?.perfilNombre || 'Sin perfil';
  }

  /** Iniciales para el avatar del cabezal (máx. 2: primer nombre + apellido). */
  get userInitials(): string {
    const partes = this.userName.trim().split(/\s+/).filter(Boolean);
    if (partes.length === 0) return '?';
    const primera = partes[0][0] ?? '';
    const segunda = partes.length > 1 ? partes[partes.length - 1][0] ?? '' : '';
    return (primera + segunda).toUpperCase();
  }

  private updatePageTitle(url: string): void {
    const segments = url.split('?')[0].split('/').filter(s => s);
    if (segments.length === 0) {
      this.currentPageTitle.set('Inicio');
      this.currentRoute.set('');
      this.currentPageSufijo.set('');
      return;
    }

    // Ficha de detalle (/envio/1): el id crudo NO es título ni miga — la ruta
    // lógica es la de la entidad y el rastro cierra con "Detalle".
    const lastSegment = segments[segments.length - 1];
    const esId = /^\d+$/.test(lastSegment);
    const base = esId && segments.length > 1 ? segments[segments.length - 2] : lastSegment;
    this.currentPageSufijo.set(esId ? 'Detalle' : '');

    const menuItem = this.menuItems.find(item => item.path === base);

    if (menuItem) {
      this.currentPageTitle.set(menuItem.label);
      this.currentRoute.set(menuItem.path);
    } else {
      this.currentPageTitle.set(
        base.charAt(0).toUpperCase() + base.slice(1)
      );
      this.currentRoute.set(base);
    }
  }

  toggleSidebar(): void {
    this.sidebarCollapsed.update(v => !v);
  }

  navigateTo(path: string): void {
    this.currentRoute.set(path);
    this.router.navigate(['/', path]);
  }

  isActiveRoute(path: string): boolean {
    return this.currentRoute() === path;
  }

  isInSeguridad(): boolean {
    return this.currentRoute().startsWith('seguridad');
  }

  toggleSeguridad(): void {
    this.seguridadExpanded.update(v => !v);
  }

  toggleGroup(label: string): void {
    this.expandedGroups.update(groups => {
      const next = new Set(groups);
      if (next.has(label)) {
        next.delete(label);
      } else {
        next.add(label);
      }
      return next;
    });
  }

  isGroupExpanded(label: string): boolean {
    return this.expandedGroups().has(label);
  }

  isInGroup(group: GeneratedMenuGroup): boolean {
    return group.items.some(item => this.currentRoute() === item.path);
  }

  navigateToSeguridad(path: string): void {
    this.currentRoute.set(path);
    this.router.navigate(['/', ...path.split('/')]);
  }

  logout(): void {
    this.authService.logout();
  }

  toggleTheme(): void {
    this.themeService.toggle();
  }

  cycleTheme(): void {
    this.themeService.cycle();
  }
}
