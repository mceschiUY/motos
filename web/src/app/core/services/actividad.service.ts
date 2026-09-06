import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { BUSQUEDA_ENTIDADES, BusquedaEntidad } from '../../modules/generated/generated-search.registry';

export interface EventoNegocio {
  fecha: string;
  quien: string;
  frase: string;
  ruta: string;
  id: string | number | null;
}

/**
 * El pulso del negocio: traduce la auditoría (que el sistema registra desde siempre)
 * a frases de negocio. Solo eventos de entidades del PRODUCTO (el manifiesto de
 * búsqueda filtra el ruido de sistema/seguridad).
 */
@Injectable({ providedIn: 'root' })
export class ActividadService {
  private readonly http = inject(HttpClient);

  readonly eventos = signal<EventoNegocio[]>([]);
  readonly hayNovedades = signal(false);

  private readonly porApi = new Map<string, BusquedaEntidad>(
    BUSQUEDA_ENTIDADES.map(e => [e.api, e])
  );

  cargar(cantidad = 30): void {
    this.http.get<any[]>(`${environment.apiUrl}/Auditoria/recientes`, { params: { cantidad } }).subscribe({
      next: (logs) => {
        const eventos = (logs || [])
          .filter(l => l.success !== false)
          .map(l => this.traducir(l))
          .filter((e): e is EventoNegocio => e !== null);
        this.eventos.set(eventos);

        const ultimoVisto = Number(localStorage.getItem('actividad-ultimo-visto') || 0);
        this.hayNovedades.set(eventos.some(e => new Date(e.fecha).getTime() > ultimoVisto));
      },
      error: () => this.eventos.set([])
    });
  }

  marcarVisto(): void {
    localStorage.setItem('actividad-ultimo-visto', String(Date.now()));
    this.hayNovedades.set(false);
  }

  private traducir(l: any): EventoNegocio | null {
    const ent = this.porApi.get((l.entityType || '').toLowerCase());
    if (!ent) { return null; }

    const quien = l.userName || 'alguien';
    const ref = ent.label + (l.entityId ? ' #' + l.entityId : '');
    const accion = l.action || '';

    let frase: string;
    const paso = accion.match(/PasarA([A-Za-z]+)/);
    if (paso) {
      const destino = paso[1].replace(/([A-Z])/g, ' $1').trim().toLowerCase();
      frase = `pasó ${ref} a ${destino}`;
    } else if (accion.includes('Crear')) {
      frase = `creó ${ref}`;
    } else if (accion.includes('Modificar')) {
      frase = `modificó ${ref}`;
    } else if (accion.includes('Eliminar')) {
      frase = `eliminó ${ref}`;
    } else {
      frase = `${accion} — ${ref}`;
    }

    return { fecha: l.timestamp, quien, frase, ruta: ent.ruta, id: l.entityId ?? null };
  }
}
