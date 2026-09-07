import { Injectable, inject } from '@angular/core';
import { GeneratedFacadeBase } from '../../../core/services/generated-facade.base';
import { Talla } from '../models/talla.model';
import { TallaApiService } from './talla.api.service';

@Injectable({
  providedIn: 'root'
})
export class TallaService extends GeneratedFacadeBase<Talla, TallaApiService> {
  protected readonly apiService = inject(TallaApiService);
}
