import { Injectable, inject } from '@angular/core';
import { GeneratedFacadeBase } from '../../../core/services/generated-facade.base';
import { Cliente } from '../models/cliente.model';
import { ClienteApiService } from './cliente.api.service';

@Injectable({
  providedIn: 'root'
})
export class ClienteService extends GeneratedFacadeBase<Cliente, ClienteApiService> {
  protected readonly apiService = inject(ClienteApiService);
}
