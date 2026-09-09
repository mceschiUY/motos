import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { Existencia } from './existencia.model';

/**
 * Query artesanal read-only (contrato artesanal 2026-08-29: datos SOLO por facades
 * existentes o queries read-only del backend). No toca los shells generados de
 * MovimientoStock: consume directo el endpoint de lectura.
 */
@Injectable({ providedIn: 'root' })
export class ExistenciasService {
  private readonly http = inject(HttpClient);
  private readonly url = `${environment.apiUrl}/MovimientoStock/existencias`;

  existencias(varianteId?: number | null, depositoId?: number | null): Observable<Existencia[]> {
    let params = new HttpParams();
    if (varianteId != null) params = params.set('varianteId', varianteId);
    if (depositoId != null) params = params.set('depositoId', depositoId);
    return this.http.get<Existencia[]>(this.url, { params });
  }
}
