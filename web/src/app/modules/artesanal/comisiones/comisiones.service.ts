import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { ComisionVendedor } from './comision.model';

/**
 * Query artesanal read-only (contrato artesanal 2026-08-29). Consume el endpoint de
 * liquidación del backend; no toca los shells generados de Pedido.
 */
@Injectable({ providedIn: 'root' })
export class ComisionesService {
  private readonly http = inject(HttpClient);
  private readonly url = `${environment.apiUrl}/artesanal/comisiones`;

  comisiones(periodo?: string | null, vendedorId?: number | null): Observable<ComisionVendedor[]> {
    let params = new HttpParams();
    if (periodo) { params = params.set('periodo', periodo); }
    if (vendedorId != null) { params = params.set('vendedorId', vendedorId); }
    return this.http.get<ComisionVendedor[]>(this.url, { params });
  }
}
