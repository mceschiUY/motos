import { Injectable } from '@angular/core';
import { GeneratedApiBase } from '../../../core/services/generated-api.base';
import { Agencia } from '../models/agencia.model';

@Injectable({
  providedIn: 'root'
})
export class AgenciaApiService extends GeneratedApiBase<Agencia> {
  protected readonly recurso = 'Agencia';
}
