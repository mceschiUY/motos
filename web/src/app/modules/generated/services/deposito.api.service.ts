import { Injectable } from '@angular/core';
import { GeneratedApiBase } from '../../../core/services/generated-api.base';
import { Deposito } from '../models/deposito.model';

@Injectable({
  providedIn: 'root'
})
export class DepositoApiService extends GeneratedApiBase<Deposito> {
  protected readonly recurso = 'Deposito';
}
