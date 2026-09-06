import { Injectable } from '@angular/core';
import { GeneratedApiBase } from '../../../core/services/generated-api.base';
import { Parametrosla } from '../models/parametrosla.model';

@Injectable({
  providedIn: 'root'
})
export class ParametroslaApiService extends GeneratedApiBase<Parametrosla> {
  protected readonly recurso = 'Parametrosla';
}
