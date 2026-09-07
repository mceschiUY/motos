import { Injectable } from '@angular/core';
import { GeneratedApiBase } from '../../../core/services/generated-api.base';
import { Talla } from '../models/talla.model';

@Injectable({
  providedIn: 'root'
})
export class TallaApiService extends GeneratedApiBase<Talla> {
  protected readonly recurso = 'Talla';
}
