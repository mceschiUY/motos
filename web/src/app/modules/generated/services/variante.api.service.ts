import { Injectable } from '@angular/core';
import { GeneratedApiBase } from '../../../core/services/generated-api.base';
import { Variante } from '../models/variante.model';

@Injectable({
  providedIn: 'root'
})
export class VarianteApiService extends GeneratedApiBase<Variante> {
  protected readonly recurso = 'Variante';
}
