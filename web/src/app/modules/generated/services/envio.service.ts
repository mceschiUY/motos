import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { GeneratedFacadeBase } from '../../../core/services/generated-facade.base';
import { Envio } from '../models/envio.model';
import { EnvioApiService } from './envio.api.service';

@Injectable({
  providedIn: 'root'
})
export class EnvioService extends GeneratedFacadeBase<Envio, EnvioApiService> {
  protected readonly apiService = inject(EnvioApiService);

  getByClienteId(clienteId: number): Observable<Envio[]> {
    return this.apiService.getByClienteId(clienteId);
  }

  getByAgenciaId(agenciaId: number): Observable<Envio[]> {
    return this.apiService.getByAgenciaId(agenciaId);
  }

  pasarAFacturado(id: string | number): Observable<boolean> {
    return this.apiService.pasarAFacturado(id);
  }

  pasarADespachado(id: string | number): Observable<boolean> {
    return this.apiService.pasarADespachado(id);
  }

  pasarAEntregado(id: string | number): Observable<boolean> {
    return this.apiService.pasarAEntregado(id);
  }

  pasarAAnulado(id: string | number): Observable<boolean> {
    return this.apiService.pasarAAnulado(id);
  }
}
