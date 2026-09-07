import { Injectable } from '@angular/core';
import { GeneratedApiBase } from '../../../core/services/generated-api.base';
import { MovimientoStock } from '../models/movimientostock.model';

@Injectable({
  providedIn: 'root'
})
export class MovimientoStockApiService extends GeneratedApiBase<MovimientoStock> {
  protected readonly recurso = 'MovimientoStock';
}
