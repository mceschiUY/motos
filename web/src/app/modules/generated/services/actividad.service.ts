import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { GeneratedFacadeBase } from '../../../core/services/generated-facade.base';
import { Actividad } from '../models/actividad.model';
import { ActividadApiService } from './actividad.api.service';

@Injectable({
  providedIn: 'root'
})
export class ActividadService extends GeneratedFacadeBase<Actividad, ActividadApiService> {
  protected readonly apiService = inject(ActividadApiService);

  getByVendedorId(vendedorId: number): Observable<Actividad[]> {
    return this.apiService.getByVendedorId(vendedorId);
  }

  getByClienteId(clienteId: number): Observable<Actividad[]> {
    return this.apiService.getByClienteId(clienteId);
  }
}
