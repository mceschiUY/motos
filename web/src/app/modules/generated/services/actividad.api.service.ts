import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { GeneratedApiBase } from '../../../core/services/generated-api.base';
import { Actividad } from '../models/actividad.model';

@Injectable({
  providedIn: 'root'
})
export class ActividadApiService extends GeneratedApiBase<Actividad> {
  protected readonly recurso = 'Actividad';

  getByVendedorId(vendedorId: number): Observable<Actividad[]> {
    return this.byFk('vendedor', vendedorId);
  }

  getByClienteId(clienteId: number): Observable<Actividad[]> {
    return this.byFk('cliente', clienteId);
  }
}
