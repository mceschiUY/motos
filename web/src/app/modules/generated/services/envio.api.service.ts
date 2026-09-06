import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { GeneratedApiBase } from '../../../core/services/generated-api.base';
import { Envio } from '../models/envio.model';

@Injectable({
  providedIn: 'root'
})
export class EnvioApiService extends GeneratedApiBase<Envio> {
  protected readonly recurso = 'Envio';

  getByClienteId(clienteId: number): Observable<Envio[]> {
    return this.byFk('cliente', clienteId);
  }

  getByAgenciaId(agenciaId: number): Observable<Envio[]> {
    return this.byFk('agencia', agenciaId);
  }

  // Accion Marcar Facturado; el servidor sella la fecha de factura
  pasarAFacturado(id: string | number): Observable<boolean> {
    return this.accion(id, 'pasar-a-facturado');
  }

  // Accion Despachar; el servidor sella la fecha de envio
  pasarADespachado(id: string | number): Observable<boolean> {
    return this.accion(id, 'pasar-a-despachado');
  }

  // Accion Confirmar Entrega; el servidor sella la fecha de entrega y cierra el ciclo
  pasarAEntregado(id: string | number): Observable<boolean> {
    return this.accion(id, 'pasar-a-entregado');
  }

  // Motivo de anulacion obligatorio
  pasarAAnulado(id: string | number): Observable<boolean> {
    return this.accion(id, 'pasar-a-anulado');
  }
}
