import { Injectable, inject } from '@angular/core';
import { GeneratedFacadeBase } from '../../../core/services/generated-facade.base';
import { Parametrosla } from '../models/parametrosla.model';
import { ParametroslaApiService } from './parametrosla.api.service';

@Injectable({
  providedIn: 'root'
})
export class ParametroslaService extends GeneratedFacadeBase<Parametrosla, ParametroslaApiService> {
  protected readonly apiService = inject(ParametroslaApiService);
}
