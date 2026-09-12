import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { SeguimientoEnvio } from './seguimiento.model';

/**
 * Queries artesanales read-only del seguimiento del envío. Las transiciones del ciclo NO
 * pasan por acá: van por EnvioApiService (POST api/Envio/{id}/{accion}) y la observación
 * nueva por el form generado de Observación.
 */
@Injectable({ providedIn: 'root' })
export class SeguimientoService {
  private readonly http = inject(HttpClient);

  /** Interno (con login): `api/artesanal/envio/{id}/seguimiento`, con teléfono, precios y usuarios. */
  interno(envioId: number): Observable<SeguimientoEnvio> {
    return this.http.get<SeguimientoEnvio>(`${environment.apiUrl}/artesanal/envio/${envioId}/seguimiento`);
  }

  /** Público (sin login): `api/publico/seguimiento/{codigo}`, recortado. 404 si el código no existe. */
  publico(codigo: string): Observable<SeguimientoEnvio> {
    return this.http.get<SeguimientoEnvio>(`${environment.apiUrl}/publico/seguimiento/${encodeURIComponent(codigo.trim())}`);
  }

  /** Link que se comparte con el cliente (QR y botón copiar). */
  linkPublico(codigo: string): string {
    return `${location.origin}/seguimiento/${encodeURIComponent(codigo.trim())}`;
  }
}
