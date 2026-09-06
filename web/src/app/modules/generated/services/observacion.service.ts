import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { GeneratedFacadeBase } from '../../../core/services/generated-facade.base';
import { Observacion } from '../models/observacion.model';
import { ObservacionApiService } from './observacion.api.service';

@Injectable({
  providedIn: 'root'
})
export class ObservacionService extends GeneratedFacadeBase<Observacion, ObservacionApiService> {
  protected readonly apiService = inject(ObservacionApiService);

  getByEnvioId(envioId: number): Observable<Observacion[]> {
    return this.apiService.getByEnvioId(envioId);
  }
}
