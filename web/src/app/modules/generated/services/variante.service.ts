import { Injectable, inject } from '@angular/core';
import { GeneratedFacadeBase } from '../../../core/services/generated-facade.base';
import { Variante } from '../models/variante.model';
import { VarianteApiService } from './variante.api.service';

@Injectable({
  providedIn: 'root'
})
export class VarianteService extends GeneratedFacadeBase<Variante, VarianteApiService> {
  protected readonly apiService = inject(VarianteApiService);
}
