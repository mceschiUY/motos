import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { AvanceVendedor, ClienteSinVisitar, ParadaAgenda } from './agenda.model';

/** Queries artesanales read-only de la fuerza de ventas (HttpClient directo, sin tocar shells). */
@Injectable({ providedIn: 'root' })
export class AgendaService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/artesanal`;

  agenda(vendedorId: number | null, desde: string, hasta: string): Observable<ParadaAgenda[]> {
    let params = new HttpParams().set('desde', desde).set('hasta', hasta);
    if (vendedorId != null) params = params.set('vendedorId', vendedorId);
    return this.http.get<ParadaAgenda[]>(`${this.base}/agenda`, { params });
  }

  avance(vendedorId: number, periodo?: string): Observable<AvanceVendedor> {
    let params = new HttpParams();
    if (periodo) params = params.set('periodo', periodo);
    return this.http.get<AvanceVendedor>(`${this.base}/vendedor/${vendedorId}/avance`, { params });
  }

  clientesSinVisitar(dias = 30): Observable<ClienteSinVisitar[]> {
    return this.http.get<ClienteSinVisitar[]>(`${this.base}/alertas/clientes-sin-visitar`, { params: new HttpParams().set('dias', dias) });
  }
}
