import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { GeneratedApiBase } from '../../../core/services/generated-api.base';
import { PedidoLinea } from '../models/pedidolinea.model';

@Injectable({
  providedIn: 'root'
})
export class PedidoLineaApiService extends GeneratedApiBase<PedidoLinea> {
  protected readonly recurso = 'PedidoLinea';

  getByPedidoId(pedidoId: number): Observable<PedidoLinea[]> {
    return this.byFk('pedido', pedidoId);
  }
}
