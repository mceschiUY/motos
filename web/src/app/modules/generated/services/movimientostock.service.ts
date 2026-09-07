import { Injectable, inject } from '@angular/core';
import { GeneratedFacadeBase } from '../../../core/services/generated-facade.base';
import { MovimientoStock } from '../models/movimientostock.model';
import { MovimientoStockApiService } from './movimientostock.api.service';

@Injectable({
  providedIn: 'root'
})
export class MovimientoStockService extends GeneratedFacadeBase<MovimientoStock, MovimientoStockApiService> {
  protected readonly apiService = inject(MovimientoStockApiService);
}
