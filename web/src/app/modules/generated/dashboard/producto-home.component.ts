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
    { api: 'marca', ruta: '/marca', label: 'Marca', icon: 'branding_watermark', modulo: 'Catálogo', conEstado: false, conMes: false },
    { api: 'categoria', ruta: '/categoria', label: 'Categoría', icon: 'category', modulo: 'Catálogo', conEstado: false, conMes: false },
    { api: 'talla', ruta: '/talla', label: 'Talla', icon: 'straighten', modulo: 'Catálogo', conEstado: false, conMes: false },
    { api: 'color', ruta: '/color', label: 'Color', icon: 'palette', modulo: 'Catálogo', conEstado: false, conMes: false },
    { api: 'producto', ruta: '/producto', label: 'Producto', icon: 'inventory_2', modulo: 'Catálogo', conEstado: false, conMes: false },
    { api: 'variante', ruta: '/variante', label: 'Variante', icon: 'qr_code_2', modulo: 'Catálogo', conEstado: false, conMes: false },
    { api: 'deposito', ruta: '/deposito', label: 'Depósito', icon: 'warehouse', modulo: 'Inventario', conEstado: false, conMes: false },
    { api: 'movimientostock', ruta: '/movimientostock', label: 'Movimiento de stock', icon: 'swap_vert', modulo: 'Inventario', conEstado: false, conMes: true },
    { api: 'agencia', ruta: '/agencia', label: 'Agencia', icon: 'list_alt', modulo: 'Operaciones', conEstado: false, conMes: false },
    { api: 'cliente', ruta: '/cliente', label: 'Cliente', icon: 'person', modulo: 'Comercial', conEstado: false, conMes: false },
    { api: 'vendedor', ruta: '/vendedor', label: 'Vendedor', icon: 'badge', modulo: 'Comercial', conEstado: false, conMes: false },
    { api: 'actividad', ruta: '/actividad', label: 'Actividad', icon: 'event_note', modulo: 'Comercial', conEstado: true, conMes: true },
    { api: 'meta', ruta: '/meta', label: 'Meta', icon: 'flag', modulo: 'Comercial', conEstado: false, conMes: true },
    { api: 'pedido', ruta: '/pedido', label: 'Pedido', icon: 'receipt_long', modulo: 'Comercial', conEstado: true, conMes: true },
    { api: 'envio', ruta: '/envio', label: 'Envio', icon: 'list_alt', modulo: 'Operaciones', conEstado: true, conMes: true },
    { api: 'observacion', ruta: '/observacion', label: 'Observación', icon: 'list_alt', modulo: 'Operaciones', conEstado: false, conMes: true },
    { api: 'parametrosla', ruta: '/parametrosla', label: 'Parametro SLA', icon: 'list_alt', modulo: 'Operaciones', conEstado: true, conMes: false }
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
