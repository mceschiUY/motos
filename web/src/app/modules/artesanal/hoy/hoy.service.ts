import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { CentroControl } from './hoy.model';

/**
 * Query artesanal read-only (contrato artesanal 2026-08-29). Una sola llamada trae
 * todo lo que muestra el home "Hoy" y el primer slide del modo pantalla.
 */
@Injectable({ providedIn: 'root' })
export class HoyService {
  private readonly http = inject(HttpClient);
  private readonly url = `${environment.apiUrl}/artesanal/centro-control`;

  centroControl(): Observable<CentroControl> {
    return this.http.get<CentroControl>(this.url);
  }
}
