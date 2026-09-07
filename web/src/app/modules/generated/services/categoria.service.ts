import { Injectable, inject } from '@angular/core';
import { GeneratedFacadeBase } from '../../../core/services/generated-facade.base';
import { Categoria } from '../models/categoria.model';
import { CategoriaApiService } from './categoria.api.service';

@Injectable({
  providedIn: 'root'
})
export class CategoriaService extends GeneratedFacadeBase<Categoria, CategoriaApiService> {
  protected readonly apiService = inject(CategoriaApiService);
}
