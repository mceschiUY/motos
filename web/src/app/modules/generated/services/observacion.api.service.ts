import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { GeneratedApiBase } from '../../../core/services/generated-api.base';
import { Observacion } from '../models/observacion.model';

@Injectable({
  providedIn: 'root'
})
export class ObservacionApiService extends GeneratedApiBase<Observacion> {
  protected readonly recurso = 'Observacion';

  getByEnvioId(envioId: number): Observable<Observacion[]> {
    return this.byFk('envio', envioId);
  }
}
