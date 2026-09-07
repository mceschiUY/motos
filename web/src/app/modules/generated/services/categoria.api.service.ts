import { Injectable } from '@angular/core';
import { GeneratedApiBase } from '../../../core/services/generated-api.base';
import { Categoria } from '../models/categoria.model';

@Injectable({
  providedIn: 'root'
})
export class CategoriaApiService extends GeneratedApiBase<Categoria> {
  protected readonly recurso = 'Categoria';
}
