import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { Existencia, Existencias } from './existencia.model';

/**
 * Queries artesanales read-only (contrato artesanal 2026-08-29). `agrupadas` es la consulta
 * de la escena (categoría → producto → SKU, con semáforo y cobertura); `existencias` es la
 * lectura plana original del Kardex, que sigue disponible.
 */
@Injectable({ providedIn: 'root' })
export class ExistenciasService {
  private readonly http = inject(HttpClient);

  agrupadas(depositoId?: number | null): Observable<Existencias> {
    let params = new HttpParams();
    if (depositoId != null) params = params.set('depositoId', depositoId);
    return this.http.get<Existencias>(`${environment.apiUrl}/artesanal/existencias`, { params });
  }

  existencias(varianteId?: number | null, depositoId?: number | null): Observable<Existencia[]> {
    let params = new HttpParams();
    if (varianteId != null) params = params.set('varianteId', varianteId);
    if (depositoId != null) params = params.set('depositoId', depositoId);
    return this.http.get<Existencia[]>(`${environment.apiUrl}/MovimientoStock/existencias`, { params });
  }
}
