import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { GeneratedApiBase } from '../../../core/services/generated-api.base';
import { Cliente } from '../models/cliente.model';

@Injectable({
  providedIn: 'root'
})
export class ClienteApiService extends GeneratedApiBase<Cliente> {
  protected readonly recurso = 'Cliente';

  getByVendedorId(vendedorId: number): Observable<Cliente[]> {
    return this.byFk('vendedor', vendedorId);
  }
}
