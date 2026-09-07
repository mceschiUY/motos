import { Injectable } from '@angular/core';
import { GeneratedApiBase } from '../../../core/services/generated-api.base';
import { Producto } from '../models/producto.model';

@Injectable({
  providedIn: 'root'
})
export class ProductoApiService extends GeneratedApiBase<Producto> {
  protected readonly recurso = 'Producto';
}
