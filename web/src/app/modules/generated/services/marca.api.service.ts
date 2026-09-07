import { Injectable } from '@angular/core';
import { GeneratedApiBase } from '../../../core/services/generated-api.base';
import { Marca } from '../models/marca.model';

@Injectable({
  providedIn: 'root'
})
export class MarcaApiService extends GeneratedApiBase<Marca> {
  protected readonly recurso = 'Marca';
}
