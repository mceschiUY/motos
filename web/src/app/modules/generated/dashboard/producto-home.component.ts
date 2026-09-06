import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { environment } from '../../../../environments/environment';

interface GrupoConteo { clave: string; cantidad: number; }
interface Resumen { total: number; porEstado: GrupoConteo[]; porMes: GrupoConteo[]; }
interface HomeEntidad { api: string; ruta: string; label: string; icon: string; modulo: string; conEstado: boolean; conMes: boolean; }

@Component({
  selector: 'app-producto-home',
  standalone: true,
  imports: [CommonModule, RouterModule, MatIconModule, MatTooltipModule],
  templateUrl: './producto-home.component.html',
  styleUrl: './producto-home.component.scss'
})
export class ProductoHomeComponent implements OnInit {
  private readonly http = inject(HttpClient);

  readonly titulo = 'Sistema web para gestionar envios y su trazabilidad reemplazando la planilla Excel: cada e';
  readonly entidades: HomeEntidad[] = [
    { api: 'agencia', ruta: '/agencia', label: 'Agencia', icon: 'list_alt', modulo: 'Gestion de agencias', conEstado: false, conMes: false },
    { api: 'cliente', ruta: '/cliente', label: 'Cliente', icon: 'person', modulo: 'Gestion de clientes', conEstado: false, conMes: false },
    { api: 'envio', ruta: '/envio', label: 'Envio', icon: 'list_alt', modulo: 'Gestion de clientes', conEstado: true, conMes: true },
    { api: 'observacion', ruta: '/observacion', label: 'Observación', icon: 'list_alt', modulo: 'Operaciones y flujo de estados del envio', conEstado: false, conMes: true },
    { api: 'parametrosla', ruta: '/parametrosla', label: 'Parametro SLA', icon: 'list_alt', modulo: 'General', conEstado: true, conMes: false }
  ];

  readonly resumenes = signal<Record<string, Resumen | null>>({});

  ngOnInit(): void {
    for (const e of this.entidades) {
      this.http.get<Resumen>(`${environment.apiUrl}/${e.api}/resumen`).subscribe({
        next: (r: Resumen) => this.resumenes.update(m => ({ ...m, [e.api]: r })),
        error: () => this.resumenes.update(m => ({ ...m, [e.api]: null }))
      });
    }
  }

  resumenDe(api: string): Resumen | null {
    return this.resumenes()[api] ?? null;
  }

  maxDe(grupos: GrupoConteo[]): number {
    return Math.max(1, ...grupos.map(g => g.cantidad));
  }

  etiquetaMes(clave: string): string {
    const partes = (clave || '').split('-');
    const meses = ['Ene', 'Feb', 'Mar', 'Abr', 'May', 'Jun', 'Jul', 'Ago', 'Set', 'Oct', 'Nov', 'Dic'];
    const m = parseInt(partes[1], 10);
    return m >= 1 && m <= 12 ? meses[m - 1] : clave;
  }
}
