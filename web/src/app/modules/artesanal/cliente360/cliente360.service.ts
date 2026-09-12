import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { Cliente360 } from './cliente360.model';

/**
 * Query artesanal read-only de la escena Cliente 360 (contrato artesanal: lectura por
 * endpoints propios; las escrituras siguen yendo por los forms/commands generados).
 */
@Injectable({ providedIn: 'root' })
export class Cliente360Service {
  private readonly http = inject(HttpClient);
  private readonly url = `${environment.apiUrl}/artesanal/cliente`;

  cliente360(clienteId: number): Observable<Cliente360> {
    return this.http.get<Cliente360>(`${this.url}/${clienteId}/360`);
  }
}
