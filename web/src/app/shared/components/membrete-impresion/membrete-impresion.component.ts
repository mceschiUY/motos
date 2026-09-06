import { Component, Input, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SiteConfigService } from '../../../core/services/site-config.service';

/**
 * Membrete de impresión: la marca del negocio arriba de la hoja cuando la ficha
 * se imprime o se guarda como PDF (botón PDF → window.print()). Invisible en
 * pantalla; en papel encabeza con logo, nombre, contacto y fecha de emisión.
 */
@Component({
  selector: 'app-membrete-impresion',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="mi-membrete">
      <div class="mi-marca">
        @if (config.sitioLogoUrl()) { <img class="mi-logo" [src]="config.sitioLogoUrl()" alt="" /> }
        <div class="mi-nombres">
          <span class="mi-sitio">{{ config.empresaNombre() || config.sitioNombre() }}</span>
          <span class="mi-contacto">
            @if (config.empresaTelefono()) { {{ config.empresaTelefono() }} }
            @if (config.empresaTelefono() && config.empresaEmail()) { · }
            @if (config.empresaEmail()) { {{ config.empresaEmail() }} }
          </span>
        </div>
      </div>
      <div class="mi-doc">
        @if (tipo) { <span class="mi-tipo">{{ tipo }}</span> }
        @if (titulo) { <span class="mi-titulo">{{ titulo }}</span> }
        <span class="mi-fecha">Emitido el {{ hoy | date:'dd/MM/yyyy HH:mm' }}</span>
      </div>
    </div>
  `,
  styles: [`
    :host { display: none; }
    @media print {
      :host { display: block; }
      .mi-membrete { display: flex; justify-content: space-between; align-items: flex-end; gap: 24px; padding-bottom: 10px; margin-bottom: 18px; border-bottom: 2px solid #333; }
      .mi-marca { display: flex; align-items: center; gap: 12px; }
      .mi-logo { height: 42px; width: auto; object-fit: contain; }
      .mi-nombres { display: flex; flex-direction: column; }
      .mi-sitio { font-size: 18px; font-weight: 700; color: #111; }
      .mi-contacto { font-size: 11px; color: #555; }
      .mi-doc { display: flex; flex-direction: column; align-items: flex-end; text-align: right; }
      .mi-tipo { font-size: 10px; text-transform: uppercase; letter-spacing: .1em; color: #777; }
      .mi-titulo { font-size: 13px; font-weight: 600; color: #111; }
      .mi-fecha { font-size: 10px; color: #777; }
    }
  `]
})
export class MembreteImpresionComponent {
  readonly config = inject(SiteConfigService);

  @Input() titulo = '';
  @Input() tipo = '';

  readonly hoy = new Date();
}
