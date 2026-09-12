import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { MatButtonModule } from '@angular/material/button';
import { AuthService } from '../../../core/services/auth.service';
import { SiteConfigService } from '../../../core/services/site-config.service';
import { ThemeService } from '../../../core/services/theme.service';
import { VendedorService } from '../../generated/services/vendedor.service';
import { Vendedor } from '../../generated/models/vendedor.model';
import { ModoMovilService } from './modo-movil.service';

/**
 * Shell de la app del vendedor (`/m`, plan Etapa F4): cabezal chico con el nombre del vendedor
 * y barra inferior de cinco destinos. Sin sidebar, sin Administración ni Sistema. Las
 * pantallas son las mismas escenas artesanales del sitio, montadas como rutas hijas.
 */
@Component({
  selector: 'app-movil-layout',
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterLink, RouterLinkActive, MatIconModule, MatMenuModule, MatButtonModule],
  template: `
    <div class="mv">
      <header class="mv-head">
        <div class="mv-marca">
          <span class="mv-sitio">{{ config.sitioNombre() }}</span>
          <span class="mv-quien">
            {{ vendedor()?.nombre || nombreUsuario }}
            @if (vendedor()?.zona) { <small> · {{ vendedor()!.zona }}</small> }
          </span>
        </div>
        <button mat-icon-button [matMenuTriggerFor]="menu" aria-label="Más opciones"><mat-icon>more_vert</mat-icon></button>
        <mat-menu #menu="matMenu" xPosition="before">
          <button mat-menu-item (click)="tema.cycle()">
            <mat-icon>contrast</mat-icon><span>Tema: {{ tema.mode() }}</span>
          </button>
          @if (vendedor(); as v) {
            <button mat-menu-item [routerLink]="['/m/vendedor', v.id]"><mat-icon>badge</mat-icon><span>Mi panel</span></button>
          }
          <button mat-menu-item (click)="sitioCompleto()"><mat-icon>desktop_windows</mat-icon><span>Sitio completo</span></button>
          <button mat-menu-item (click)="auth.logout()"><mat-icon>logout</mat-icon><span>Cerrar sesión</span></button>
        </mat-menu>
      </header>

      <main class="mv-main"><router-outlet></router-outlet></main>

      <nav class="mv-nav" aria-label="Navegación principal">
        <a routerLink="/m/agenda" routerLinkActive="activa"><mat-icon>route</mat-icon><span>Mi día</span></a>
        <a routerLink="/m/clientes" routerLinkActive="activa"><mat-icon>storefront</mat-icon><span>Clientes</span></a>
        <a routerLink="/m/catalogo" routerLinkActive="activa"><mat-icon>inventory_2</mat-icon><span>Catálogo</span></a>
        <a routerLink="/m/pedidos/nuevo" routerLinkActive="activa" class="mv-nav-primaria"><mat-icon>add_shopping_cart</mat-icon><span>Vender</span></a>
        <a routerLink="/m/pedidos" routerLinkActive="activa" [routerLinkActiveOptions]="{ exact: true }"><mat-icon>receipt_long</mat-icon><span>Pedidos</span></a>
      </nav>
    </div>
  `,
  styles: [`
    :host { display: block; }
    .mv { min-height: 100dvh; display: flex; flex-direction: column; background: var(--ceskia-base); color: var(--ceskia-text-primary); }
    .mv-head {
      position: sticky; top: 0; z-index: 30;
      display: flex; align-items: center; justify-content: space-between; gap: 8px;
      padding: calc(8px + env(safe-area-inset-top, 0px)) 8px 8px 16px;
      background: var(--ceskia-surface); border-bottom: 1px solid var(--ceskia-border-subtle);
    }
    .mv-marca { display: flex; flex-direction: column; min-width: 0; }
    .mv-sitio { font-size: var(--ceskia-text-xs); text-transform: uppercase; letter-spacing: .08em; color: var(--ceskia-text-tertiary); }
    .mv-quien { font-weight: var(--ceskia-font-semibold); white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
    .mv-quien small { color: var(--ceskia-text-tertiary); font-weight: 400; }
    .mv-main { flex: 1; padding: 12px 12px calc(76px + env(safe-area-inset-bottom, 0px)); }
    /* Las escenas traen su propio padding en .kosmos-page: en el celular se achica. */
    .mv-main ::ng-deep .kosmos-page { padding: 0; }
    /* Los FAB de las escenas (registrar visita, exportar…) suben por encima de la barra inferior. */
    .mv-main ::ng-deep .floating-actions { bottom: calc(84px + env(safe-area-inset-bottom, 0px)); right: 12px; }
    .mv-nav {
      position: fixed; left: 0; right: 0; bottom: 0; z-index: 30;
      display: grid; grid-template-columns: repeat(5, 1fr);
      padding: 6px 4px calc(6px + env(safe-area-inset-bottom, 0px));
      background: var(--ceskia-elevated); border-top: 1px solid var(--ceskia-border-default); box-shadow: var(--ceskia-shadow-lg);
    }
    .mv-nav a {
      display: flex; flex-direction: column; align-items: center; gap: 2px; padding: 6px 0 4px;
      color: var(--ceskia-text-tertiary); text-decoration: none; font-size: 11px; border-radius: var(--ceskia-radius-md);
    }
    .mv-nav a mat-icon { font-size: 24px; width: 24px; height: 24px; }
    .mv-nav a.activa { color: var(--ceskia-accent-primary); background: var(--ceskia-accent-primary-glow); }
    .mv-nav .mv-nav-primaria mat-icon { color: var(--ceskia-accent-primary); }
    @media (min-width: 900px) {
      /* En escritorio: ancho de teléfono centrado, para ensayar sin el modo dispositivo. */
      .mv { max-width: 480px; margin: 0 auto; box-shadow: var(--ceskia-shadow-xl); }
      .mv-nav { max-width: 480px; margin: 0 auto; }
    }
  `]
})
export class MovilLayoutComponent implements OnInit {
  readonly auth = inject(AuthService);
  readonly config = inject(SiteConfigService);
  readonly tema = inject(ThemeService);
  private readonly vendedores = inject(VendedorService);
  private readonly modo = inject(ModoMovilService);
  private readonly router = inject(Router);

  readonly vendedor = signal<Vendedor | null>(null);

  get nombreUsuario(): string {
    const u = this.auth.currentUser();
    return u?.nombreCompleto || u?.nombreUsuario || 'Vendedor';
  }

  ngOnInit(): void {
    this.modo.entrar();
    this.vendedores.mio().subscribe({
      next: v => this.vendedor.set(v),
      error: () => this.vendedor.set(null),   // el usuario no es vendedor: se muestra su nombre
    });
  }

  sitioCompleto(): void {
    this.modo.salir();
    this.router.navigateByUrl('/');
  }
}
