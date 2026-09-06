import { Component, HostListener, Input, OnChanges, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { DocumentoService } from '../../../modules/generated/services/documento.service';
import { Documento } from '../../../modules/generated/models/documento.model';

interface Foto { doc: Documento; src: SafeResourceUrl | null; }

/**
 * Fotos del registro: los Documentos con MIME imagen ya asociados por
 * relacionId+relacionNombre, mostrados como galería con lightbox.
 * Sin fotos, sin sección — la ficha emite el tag siempre y esto se oculta solo.
 */
@Component({
  selector: 'app-foto-galeria',
  standalone: true,
  imports: [CommonModule, MatIconModule, MatButtonModule, MatTooltipModule],
  template: `
    @if (fotos().length > 0) {
      <section class="fg-seccion">
        <div class="fg-cabezal">
          <h2>Fotos</h2>
          <button mat-icon-button class="fg-gestionar" (click)="gestionar()" matTooltip="Agregar o quitar fotos"><mat-icon>add_photo_alternate</mat-icon></button>
        </div>
        <div class="fg-grilla">
          @for (f of fotos(); track f.doc.id; let i = $index) {
            <button class="fg-thumb" (click)="abrir(i)" [matTooltip]="f.doc.nombre">
              @if (f.src) {
                <img [src]="f.src" [alt]="f.doc.nombre" loading="lazy" />
              } @else {
                <mat-icon class="fg-cargando">image</mat-icon>
              }
            </button>
          }
        </div>
      </section>

      @if (abierta() !== null) {
        <div class="fg-lightbox" (click)="cerrar()">
          <button mat-icon-button class="fg-cerrar" (click)="cerrar()"><mat-icon>close</mat-icon></button>
          @if (fotos().length > 1) {
            <button mat-icon-button class="fg-nav fg-prev" (click)="mover(-1); $event.stopPropagation()"><mat-icon>chevron_left</mat-icon></button>
          }
          <div class="fg-marco" (click)="$event.stopPropagation()">
            @if (fotoAbierta(); as f) {
              @if (f.src) { <img [src]="f.src" [alt]="f.doc.nombre" /> }
              <span class="fg-pie">{{ f.doc.nombre }} · {{ abierta()! + 1 }} / {{ fotos().length }}</span>
            }
          </div>
          @if (fotos().length > 1) {
            <button mat-icon-button class="fg-nav fg-next" (click)="mover(1); $event.stopPropagation()"><mat-icon>chevron_right</mat-icon></button>
          }
        </div>
      }
    }
  `,
  styles: [`
    .fg-seccion { background: var(--ceskia-elevated); border: 1px solid var(--ceskia-border-subtle); border-radius: var(--ceskia-radius-lg); padding: var(--ceskia-space-5) var(--ceskia-space-6); margin-bottom: var(--ceskia-space-5); }
    .fg-cabezal { display: flex; align-items: center; justify-content: space-between; margin-bottom: var(--ceskia-space-4); }
    .fg-cabezal h2 { margin: 0; font-size: var(--ceskia-text-sm); text-transform: uppercase; letter-spacing: .08em; color: var(--ceskia-text-tertiary); }
    .fg-gestionar { color: var(--ceskia-text-tertiary); }
    .fg-grilla { display: grid; grid-template-columns: repeat(auto-fill, minmax(140px, 1fr)); gap: var(--ceskia-space-3); }
    .fg-thumb { position: relative; aspect-ratio: 4 / 3; border: 1px solid var(--ceskia-border-subtle); border-radius: var(--ceskia-radius-md); overflow: hidden; padding: 0; background: var(--ceskia-surface); cursor: zoom-in; display: flex; align-items: center; justify-content: center; }
    .fg-thumb img { width: 100%; height: 100%; object-fit: cover; display: block; transition: transform .2s ease; }
    .fg-thumb:hover img { transform: scale(1.04); }
    .fg-cargando { color: var(--ceskia-text-muted); }
    .fg-lightbox { position: fixed; inset: 0; z-index: 1200; background: rgba(6, 10, 18, .85); display: flex; align-items: center; justify-content: center; }
    .fg-marco { max-width: 88vw; max-height: 88vh; display: flex; flex-direction: column; align-items: center; gap: var(--ceskia-space-2); }
    .fg-marco img { max-width: 88vw; max-height: 80vh; object-fit: contain; border-radius: var(--ceskia-radius-md); }
    /* Blanco fijo a propósito: van sobre el overlay del lightbox, oscuro en ambos temas */
    .fg-pie { color: #ffffff; font-size: var(--ceskia-text-sm); opacity: .85; }
    .fg-cerrar { position: absolute; top: 16px; right: 16px; color: #ffffff; }
    .fg-nav { color: #ffffff; }
    .fg-prev { position: absolute; left: 16px; }
    .fg-next { position: absolute; right: 16px; }
    @media print {
      .fg-lightbox, .fg-gestionar { display: none !important; }
      .fg-seccion { break-inside: avoid; }
    }
  `]
})
export class FotoGaleriaComponent implements OnChanges {
  private readonly documentos = inject(DocumentoService);
  private readonly sanitizer = inject(DomSanitizer);
  private readonly router = inject(Router);

  @Input({ required: true }) relacionId: string | number = '';
  @Input({ required: true }) relacionNombre = '';

  readonly fotos = signal<Foto[]>([]);
  readonly abierta = signal<number | null>(null);

  ngOnChanges(): void {
    this.cargar();
  }

  private cargar(): void {
    this.fotos.set([]);
    this.abierta.set(null);
    const id = Number(this.relacionId);
    if (!id || !this.relacionNombre) { return; }
    this.documentos.getByRelacion(id, this.relacionNombre).subscribe({
      next: (docs) => {
        const imagenes = (docs || []).filter(d => (d.mimeType || '').toLowerCase().startsWith('image/'));
        this.fotos.set(imagenes.map(doc => ({ doc, src: null })));
        // el contenido viaja base64 por doc: se pide de a uno y se va pintando
        for (const doc of imagenes) {
          this.documentos.getById(doc.id).subscribe({
            next: (full) => this.fotos.update(fs => fs.map(f => f.doc.id === doc.id
              ? { ...f, src: this.sanitizer.bypassSecurityTrustResourceUrl(`data:${full.mimeType};base64,${full.contenido}`) }
              : f)),
            error: () => this.fotos.update(fs => fs.filter(f => f.doc.id !== doc.id))
          });
        }
      },
      error: () => this.fotos.set([])
    });
  }

  fotoAbierta(): Foto | null {
    const i = this.abierta();
    return i !== null ? this.fotos()[i] ?? null : null;
  }

  abrir(i: number): void { this.abierta.set(i); }
  cerrar(): void { this.abierta.set(null); }

  mover(delta: number): void {
    const total = this.fotos().length;
    const actual = this.abierta();
    if (total === 0 || actual === null) { return; }
    this.abierta.set((actual + delta + total) % total);
  }

  gestionar(): void {
    this.router.navigate(['/documento'], { queryParams: { relacionId: this.relacionId, relacionNombre: this.relacionNombre } });
  }

  @HostListener('document:keydown', ['$event'])
  teclas(ev: KeyboardEvent): void {
    if (this.abierta() === null) { return; }
    if (ev.key === 'Escape') { this.cerrar(); }
    if (ev.key === 'ArrowLeft') { this.mover(-1); }
    if (ev.key === 'ArrowRight') { this.mover(1); }
  }
}
