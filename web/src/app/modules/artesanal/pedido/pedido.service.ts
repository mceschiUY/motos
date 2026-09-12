import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { FichaPedido } from './pedido.model';

/** Query artesanal read-only de la escena "Pedido". Las escrituras van por los commands generados. */
@Injectable({ providedIn: 'root' })
export class PedidoEscenaService {
  private readonly http = inject(HttpClient);

  ficha(id: number): Observable<FichaPedido> {
    return this.http.get<FichaPedido>(`${environment.apiUrl}/artesanal/pedido/${id}/ficha`);
  }
}
