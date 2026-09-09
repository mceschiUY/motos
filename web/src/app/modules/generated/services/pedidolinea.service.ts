import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { GeneratedFacadeBase } from '../../../core/services/generated-facade.base';
import { PedidoLinea } from '../models/pedidolinea.model';
import { PedidoLineaApiService } from './pedidolinea.api.service';

@Injectable({
  providedIn: 'root'
})
export class PedidoLineaService extends GeneratedFacadeBase<PedidoLinea, PedidoLineaApiService> {
  protected readonly apiService = inject(PedidoLineaApiService);

  getByPedidoId(pedidoId: number): Observable<PedidoLinea[]> {
    return this.apiService.getByPedidoId(pedidoId);
  }
}
