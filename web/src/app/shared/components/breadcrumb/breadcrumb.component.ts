import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';

export interface BreadcrumbItem {
  label: string;
  route?: string;
  icon?: string;
}

/**
 * Decisión Pablo 2026-08-23: el rastro de navegación (casita) vive en el cabezal
 * principal del layout — este componente ya no pinta nada, pero conserva su API
 * para que las vistas (a mano y generadas) sigan compilando sin tocarlas.
 */
@Component({
  selector: 'app-breadcrumb',
  standalone: true,
  imports: [CommonModule, RouterModule, MatIconModule],
  template: ``,
  styles: [`:host { display: none; }`]
})
export class BreadcrumbComponent {
  @Input() items: BreadcrumbItem[] = [];
  @Input() subtitle?: string;
}
