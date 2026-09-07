import { Injectable, inject } from '@angular/core';
import { GeneratedFacadeBase } from '../../../core/services/generated-facade.base';
import { Marca } from '../models/marca.model';
import { MarcaApiService } from './marca.api.service';

@Injectable({
  providedIn: 'root'
})
export class MarcaService extends GeneratedFacadeBase<Marca, MarcaApiService> {
  protected readonly apiService = inject(MarcaApiService);
}
