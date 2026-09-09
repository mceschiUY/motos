import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { GeneratedFacadeBase } from '../../../core/services/generated-facade.base';
import { Pedido } from '../models/pedido.model';
import { PedidoApiService } from './pedido.api.service';

@Injectable({
  providedIn: 'root'
})
export class PedidoService extends GeneratedFacadeBase<Pedido, PedidoApiService> {
  protected readonly apiService = inject(PedidoApiService);

  getByClienteId(clienteId: number): Observable<Pedido[]> {
    return this.apiService.getByClienteId(clienteId);
  }

  getByVendedorId(vendedorId: number): Observable<Pedido[]> {
    return this.apiService.getByVendedorId(vendedorId);
  }

  pasarAConfirmado(id: string | number): Observable<boolean> {
    return this.apiService.pasarAConfirmado(id);
  }

  pasarAPreparado(id: string | number): Observable<boolean> {
    return this.apiService.pasarAPreparado(id);
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
