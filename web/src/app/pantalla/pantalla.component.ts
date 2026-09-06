import { Component, HostListener, OnDestroy, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { MatIconModule } from '@angular/material/icon';
import { environment } from '../../environments/environment';
import { BUSQUEDA_ENTIDADES, BusquedaEntidad } from '../modules/generated/generated-search.registry';
import { ActividadRecienteComponent } from '../shared/components/actividad-reciente/actividad-reciente.component';

interface GrupoConteo { clave: string; cantidad: number; }
interface Resumen { total: number; porEstado: GrupoConteo[]; porMes: GrupoConteo[]; }
interface Slide { tipo: 'numeros' | 'estado' | 'actividad'; entidad?: BusquedaEntidad; }

/**
 * MODO PANTALLA: el negocio en la TV de la oficina. Rota solo entre los números
 * del producto, la distribución por estado de las entidades con ciclo y la
 * actividad reciente. Sin menú, letra grande, auto-refresh. Esc o ✕ para salir.
 */
@Component({
  selector: 'app-pantalla',
  standalone: true,
  imports: [CommonModule, MatIconModule, ActividadRecienteComponent],
  template: `
    <div class="tv">
      <button class="tv-salir" (click)="salir()" title="Salir (Esc)"><mat-icon>close</mat-icon></button>

      @if (slideActual(); as s) {
        @switch (s.tipo) {
          @case ('numeros') {
            <div class="tv-slide">
              <h1 class="tv-titulo">El negocio, ahora</h1>
              <div class="tv-numeros">
                @for (e of entidades; track e.api) {
                  <div class="tv-numero">
                    <mat-icon>{{ e.icon }}</mat-icon>
                    <span class="tv-cifra">{{ resumenDe(e.api)?.total ?? '—' }}</span>
                    <span class="tv-label">{{ e.label }}</span>
                  </div>
                }
              </div>
            </div>
          }
          @case ('estado') {
            <div class="tv-slide">
              <h1 class="tv-titulo"><mat-icon class="tv-titulo-icon">{{ s.entidad!.icon }}</mat-icon> {{ s.entidad!.label }}</h1>
              <div class="tv-estados">
                @for (g of estadosDe(s.entidad!.api); track g.clave) {
                  <div class="tv-estado">
                    <span class="tv-estado-nombre">{{ g.clave.split('_').join(' ') || '—' }}</span>
                    <div class="tv-barra"><div class="tv-fill" [style.width.%]="pct(s.entidad!.api, g.cantidad)"></div></div>
                    <span class="tv-estado-num">{{ g.cantidad }}</span>
                  </div>
                }
              </div>
            </div>
          }
          @case ('actividad') {
            <div class="tv-slide">
              <h1 class="tv-titulo">Recién pasó</h1>
              <div class="tv-actividad">
                <app-actividad-reciente [limite]="8"></app-actividad-reciente>
              </div>
            </div>
          }
        }
      }

      <div class="tv-progreso">
        @for (s of slides(); track $index; let i = $index) {
          <span class="tv-punto" [class.activo]="i === indice()"></span>
        }
      </div>
    </div>
  `,
  styles: [`
    .tv { position: fixed; inset: 0; background: var(--ceskia-base); color: var(--ceskia-text-primary); z-index: 999; display: flex; flex-direction: column; align-items: center; justify-content: center; padding: 4vh 5vw; }
    .tv-salir { position: absolute; top: 18px; right: 18px; background: transparent; border: none; color: var(--ceskia-text-muted); cursor: pointer; opacity: .4; }
    .tv-salir:hover { opacity: 1; }
    .tv-slide { width: 100%; max-width: 1200px; animation: tv-in .6s ease; }
    @keyframes tv-in { from { opacity: 0; transform: translateY(14px); } to { opacity: 1; transform: none; } }
    .tv-titulo { font-size: 2.6rem; font-weight: 600; letter-spacing: -.02em; margin: 0 0 5vh 0; display: flex; align-items: center; gap: 16px; }
    .tv-titulo-icon { font-size: 2.4rem; width: 2.4rem; height: 2.4rem; color: var(--ceskia-accent-primary); }
    .tv-numeros { display: grid; grid-template-columns: repeat(auto-fit, minmax(220px, 1fr)); gap: 3vh 3vw; }
    .tv-numero { display: flex; flex-direction: column; align-items: center; gap: 6px; mat-icon { color: var(--ceskia-accent-primary); font-size: 2rem; width: 2rem; height: 2rem; } }
    .tv-cifra { font-size: 4.4rem; font-weight: 650; line-height: 1; color: var(--ceskia-text-primary); }
    .tv-label { font-size: 1.05rem; color: var(--ceskia-text-tertiary); }
    .tv-estados { display: flex; flex-direction: column; gap: 3vh; }
    .tv-estado { display: flex; align-items: center; gap: 28px; }
    .tv-estado-nombre { width: 240px; font-size: 1.6rem; text-transform: capitalize; color: var(--ceskia-text-secondary); }
    .tv-barra { flex: 1; height: 26px; background: var(--ceskia-elevated); border-radius: 13px; overflow: hidden; }
    .tv-fill { height: 100%; background: var(--ceskia-accent-primary); border-radius: 13px; transition: width .8s ease; }
    .tv-estado-num { width: 90px; text-align: right; font-size: 2.2rem; font-weight: 600; }
    .tv-actividad { font-size: 1.2rem; ::ng-deep .ar-frase { font-size: 1.25rem; } ::ng-deep .ar-cuando { font-size: 1rem; } }
    .tv-progreso { position: absolute; bottom: 26px; display: flex; gap: 10px; }
    .tv-punto { width: 9px; height: 9px; border-radius: 50%; background: var(--ceskia-border-strong); transition: background .3s; }
    .tv-punto.activo { background: var(--ceskia-accent-primary); }
  `]
})
export class PantallaComponent implements OnInit, OnDestroy {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);

  readonly entidades = BUSQUEDA_ENTIDADES;
  readonly indice = signal(0);
  readonly slides = signal<Slide[]>([]);
  private readonly resumenes = signal<Record<string, Resumen | null>>({});

  private rotacion: ReturnType<typeof setInterval> | null = null;
  private refresco: ReturnType<typeof setInterval> | null = null;

  ngOnInit(): void {
    const slides: Slide[] = [{ tipo: 'numeros' }];
    for (const e of this.entidades.filter(x => x.conEstado).slice(0, 4)) {
      slides.push({ tipo: 'estado', entidad: e });
    }
    slides.push({ tipo: 'actividad' });
    this.slides.set(slides);

    this.cargar();
    this.rotacion = setInterval(() => this.indice.set((this.indice() + 1) % this.slides().length), 15000);
    this.refresco = setInterval(() => this.cargar(), 60000);
  }

  ngOnDestroy(): void {
    if (this.rotacion) { clearInterval(this.rotacion); }
    if (this.refresco) { clearInterval(this.refresco); }
  }

  @HostListener('document:keydown.escape')
  salir(): void { this.router.navigate(['/']); }

  slideActual(): Slide | null { return this.slides()[this.indice()] ?? null; }

  private cargar(): void {
    for (const e of this.entidades) {
      this.http.get<Resumen>(`${environment.apiUrl}/${e.api}/resumen`).subscribe({
        next: (r) => this.resumenes.update(m => ({ ...m, [e.api]: r })),
        error: () => this.resumenes.update(m => ({ ...m, [e.api]: null }))
      });
    }
  }

  resumenDe(api: string): Resumen | null { return this.resumenes()[api] ?? null; }

  estadosDe(api: string): GrupoConteo[] { return this.resumenDe(api)?.porEstado ?? []; }

  pct(api: string, cantidad: number): number {
    const max = Math.max(1, ...this.estadosDe(api).map(g => g.cantidad));
    return (cantidad / max) * 100;
  }
}
