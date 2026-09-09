import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { GeneratedFacadeBase } from '../../../core/services/generated-facade.base';
import { Cliente } from '../models/cliente.model';
import { ClienteApiService } from './cliente.api.service';

@Injectable({
  providedIn: 'root'
})
export class ClienteService extends GeneratedFacadeBase<Cliente, ClienteApiService> {
  protected readonly apiService = inject(ClienteApiService);

  /** Cartera de un vendedor: GET /Cliente/by-vendedor/{id}. */
  getByVendedorId(vendedorId: number): Observable<Cliente[]> {
    return this.apiService.getByVendedorId(vendedorId);
  }
}
