import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { GeneratedApiBase } from '../../../core/services/generated-api.base';
import { Pedido } from '../models/pedido.model';

@Injectable({
  providedIn: 'root'
})
export class PedidoApiService extends GeneratedApiBase<Pedido> {
  protected readonly recurso = 'Pedido';

  getByClienteId(clienteId: number): Observable<Pedido[]> {
    return this.byFk('cliente', clienteId);
  }

  getByVendedorId(vendedorId: number): Observable<Pedido[]> {
    return this.byFk('vendedor', vendedorId);
  }

  // Ciclo del pedido; los efectos (Kardex, Envío, comisión) los hace el backend.
  pasarAConfirmado(id: string | number): Observable<boolean> {
    return this.accion(id, 'pasar-a-confirmado');
  }

  pasarAPreparado(id: string | number): Observable<boolean> {
    return this.accion(id, 'pasar-a-preparado');
  }

  pasarADespachado(id: string | number): Observable<boolean> {
    return this.accion(id, 'pasar-a-despachado');
  }

  pasarAEntregado(id: string | number): Observable<boolean> {
    return this.accion(id, 'pasar-a-entregado');
  }

  pasarAAnulado(id: string | number): Observable<boolean> {
    return this.accion(id, 'pasar-a-anulado');
  }
}
