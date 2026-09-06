import { Component, Input, OnChanges, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';

/**
 * Mini-mapa de la ficha: dónde queda el registro. Sin API key ni dependencias —
 * iframe de OpenStreetMap si hay coordenadas, embed de Google Maps si solo hay
 * dirección en texto. Sin datos de ubicación, sin sección.
 */
@Component({
  selector: 'app-mini-mapa',
  standalone: true,
  imports: [CommonModule, MatIconModule],
  template: `
    @if (url()) {
      <section class="mm-seccion">
        <div class="mm-cabezal">
          <h2>Ubicación</h2>
          @if (linkExterno()) {
            <a class="mm-abrir" [href]="linkExterno()" target="_blank" rel="noopener">
              abrir en el mapa <mat-icon>open_in_new</mat-icon>
            </a>
          }
        </div>
        @if (direccion) { <div class="mm-direccion">📍 {{ direccion }}</div> }
        <iframe class="mm-mapa" [src]="url()" loading="lazy" referrerpolicy="no-referrer-when-downgrade" title="Mapa de ubicación"></iframe>
      </section>
    }
  `,
  styles: [`
    .mm-seccion { background: var(--ceskia-elevated); border: 1px solid var(--ceskia-border-subtle); border-radius: var(--ceskia-radius-lg); padding: var(--ceskia-space-5) var(--ceskia-space-6); margin-bottom: var(--ceskia-space-5); }
    .mm-cabezal { display: flex; align-items: center; justify-content: space-between; margin-bottom: var(--ceskia-space-3); }
    .mm-cabezal h2 { margin: 0; font-size: var(--ceskia-text-sm); text-transform: uppercase; letter-spacing: .08em; color: var(--ceskia-text-tertiary); }
    .mm-abrir { display: inline-flex; align-items: center; gap: 4px; font-size: var(--ceskia-text-xs); color: var(--ceskia-text-tertiary); text-decoration: none; }
    .mm-abrir:hover { color: var(--ceskia-text-primary); }
    .mm-abrir mat-icon { font-size: 14px; width: 14px; height: 14px; }
    .mm-direccion { font-size: var(--ceskia-text-sm); margin-bottom: var(--ceskia-space-3); }
    .mm-mapa { width: 100%; height: 280px; border: 1px solid var(--ceskia-border-subtle); border-radius: var(--ceskia-radius-md); display: block; background: var(--ceskia-surface); }
    @media print {
      // el iframe no imprime confiable: queda la dirección en texto
      .mm-mapa, .mm-abrir { display: none !important; }
      .mm-seccion { break-inside: avoid; }
    }
  `]
})
export class MiniMapaComponent implements OnChanges {
  private readonly sanitizer = inject(DomSanitizer);

  @Input() direccion: string | null = null;
  @Input() lat: number | string | null = null;
  @Input() lng: number | string | null = null;

  readonly url = signal<SafeResourceUrl | null>(null);
  readonly linkExterno = signal<string>('');

  ngOnChanges(): void {
    const lat = this.numero(this.lat);
    const lng = this.numero(this.lng);
    const dir = (this.direccion || '').toString().trim();

    if (lat !== null && lng !== null) {
      // coordenadas → OpenStreetMap con marcador (bbox chico alrededor del punto)
      const d = 0.005;
      const bbox = `${lng - d},${lat - d},${lng + d},${lat + d}`;
      this.url.set(this.sanitizer.bypassSecurityTrustResourceUrl(
        `https://www.openstreetmap.org/export/embed.html?bbox=${bbox}&layer=mapnik&marker=${lat},${lng}`));
      this.linkExterno.set(`https://www.openstreetmap.org/?mlat=${lat}&mlon=${lng}#map=17/${lat}/${lng}`);
    } else if (dir.length > 3) {
      // dirección en texto → embed clásico de Google Maps (sin key)
      const q = encodeURIComponent(dir);
      this.url.set(this.sanitizer.bypassSecurityTrustResourceUrl(
        `https://maps.google.com/maps?q=${q}&z=15&output=embed`));
      this.linkExterno.set(`https://www.google.com/maps/search/?api=1&query=${q}`);
    } else {
      this.url.set(null);
      this.linkExterno.set('');
    }
  }

  private numero(v: number | string | null): number | null {
    if (v === null || v === undefined || v === '') { return null; }
    const n = Number(v);
    // 0,0 es "sin dato" en la práctica, no una ubicación real del negocio
    return Number.isFinite(n) && n !== 0 ? n : null;
  }
}
