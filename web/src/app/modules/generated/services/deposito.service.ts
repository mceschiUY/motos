import { Injectable, inject } from '@angular/core';
import { GeneratedFacadeBase } from '../../../core/services/generated-facade.base';
import { Deposito } from '../models/deposito.model';
import { DepositoApiService } from './deposito.api.service';

@Injectable({
  providedIn: 'root'
})
export class DepositoService extends GeneratedFacadeBase<Deposito, DepositoApiService> {
  protected readonly apiService = inject(DepositoApiService);
}
