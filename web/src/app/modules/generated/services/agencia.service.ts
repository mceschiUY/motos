import { Injectable, inject } from '@angular/core';
import { GeneratedFacadeBase } from '../../../core/services/generated-facade.base';
import { Agencia } from '../models/agencia.model';
import { AgenciaApiService } from './agencia.api.service';

@Injectable({
  providedIn: 'root'
})
export class AgenciaService extends GeneratedFacadeBase<Agencia, AgenciaApiService> {
  protected readonly apiService = inject(AgenciaApiService);
}
