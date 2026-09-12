import { Component, OnDestroy, OnInit, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { HttpErrorResponse } from '@angular/common/http';
import { SiteConfigService } from '../../../core/services/site-config.service';
import { ThemeService } from '../../../core/services/theme.service';
import { SeguimientoService } from './seguimiento.service';
import { SeguimientoEnvio, SeguimientoEtapa } from './seguimiento.model';

/**
 * Seguimiento PÚBLICO del envío (`/seguimiento/:codigo`): la página que abre el cliente de
 * la tienda desde el QR o el link, sin login ni layout, pensada para el celular.
 * Consume `api/publico/seguimiento/{codigo}` (anónimo): recorrido, agencia, ciudad de
 * destino, qué contiene el pedido y la bitácora — sin precios, teléfonos ni usuarios.
 *
 * El nombre del sitio sale de SiteConfigService (`configuracion/publica`, que tampoco pide
 * token). ThemeService se inyecta para que la página respete el tema (sistema o elegido).
 */
@Component({
  selector: 'app-seguimiento-publico',
  standalone: true,
  imports: [CommonModule, FormsModule, MatIconModule, MatButtonModule, MatProgressSpinnerModule],
  templateUrl: './seguimiento-publico.component.html',
  styleUrl: './seguimiento-publico.component.scss',
})
export class SeguimientoPublicoComponent implements OnInit, OnDestroy {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly service = inject(SeguimientoService);
  readonly siteConfig = inject(SiteConfigService);
  /** Solo por su efecto: aplica data-theme al documento aunque no haya layout. */
  private readonly theme = inject(ThemeService);

  readonly isLoading = signal(false);
  readonly noEncontrado = signal(false);
  readonly errorRed = signal(false);
  readonly datos = signal<SeguimientoEnvio | null>(null);
  readonly actualizadoEn = signal<Date | null>(null);
  /** Tick cada 30 s para que "Actualizado hace X" se mueva solo. */
  private readonly ahora = signal(new Date());
  private reloj?: ReturnType<typeof setInterval>;

  codigo = '';
  codigoBuscado = '';

  /** Atajo para el template (solo se usa dentro del @else que ya garantiza datos()). */
  get d(): SeguimientoEnvio { return this.datos()!; }

  readonly etapaEnCurso = computed<SeguimientoEtapa | null>(() => this.datos()?.etapas.find(e => e.actual && !e.cumplida) ?? null);
  readonly unidades = computed(() => (this.datos()?.pedido?.lineas ?? []).reduce((s, l) => s + Number(l.cantidad || 0), 0));

  /** Frase amable de estado para el cliente, sin jerga interna. */
  readonly titular = computed(() => {
    const d = this.datos();
    if (!d) { return ''; }
    switch (d.estado) {
      case 'recibido': return 'Recibimos tu pedido y lo estamos preparando';
      case 'facturado': return 'Tu pedido está confirmado y listo para salir';
      case 'despachado': return `Tu pedido va en camino${d.agencia.nombre ? ' con ' + d.agencia.nombre : ''}`;
      case 'entregado': return 'Tu pedido fue entregado';
      case 'anulado': return 'Este envío fue anulado';
      default: return d.estadoLabel;
    }
  });

  readonly actualizadoHace = computed(() => {
    const t = this.actualizadoEn();
    if (!t) { return ''; }
    const seg = Math.max(0, Math.round((this.ahora().getTime() - t.getTime()) / 1000));
    if (seg < 60) { return 'recién'; }
    const min = Math.round(seg / 60);
    if (min < 60) { return `hace ${min} min`; }
    const h = Math.round(min / 60);
    return h === 1 ? 'hace 1 hora' : `hace ${h} horas`;
  });

  ngOnInit(): void {
    this.reloj = setInterval(() => this.ahora.set(new Date()), 30_000);
    this.route.paramMap.subscribe(pm => {
      this.codigo = (pm.get('codigo') ?? '').trim();
      this.codigoBuscado = this.codigo;
      this.cargar();
    });
  }

  ngOnDestroy(): void { if (this.reloj) { clearInterval(this.reloj); } }

  cargar(): void {
    if (!this.codigo) { this.datos.set(null); this.noEncontrado.set(true); return; }
    this.isLoading.set(true);
    this.errorRed.set(false);
    this.service.publico(this.codigo).subscribe({
      next: (d: SeguimientoEnvio) => {
        this.datos.set(d);
        this.noEncontrado.set(false);
        this.actualizadoEn.set(new Date());
        this.ahora.set(new Date());
        this.isLoading.set(false);
      },
      error: (e: HttpErrorResponse) => {
        this.datos.set(null);
        this.noEncontrado.set(e?.status === 404 || e?.status === 400);
        this.errorRed.set(!(e?.status === 404 || e?.status === 400));
        this.isLoading.set(false);
      },
    });
  }

  buscar(): void {
    const c = this.codigoBuscado.trim();
    if (!c) { return; }
    if (c.toUpperCase() === this.codigo.toUpperCase()) { this.cargar(); return; }
    this.router.navigate(['/seguimiento', c]);
  }

  textoDias(e: SeguimientoEtapa): string {
    if (e.diasTranscurridos == null) { return ''; }
    const n = e.diasTranscurridos;
    const dias = n === 1 ? '1 día' : `${n} días`;
    return e.cumplida ? `tardó ${dias}` : (n === 0 ? 'desde hoy' : `hace ${dias}`);
  }
}
