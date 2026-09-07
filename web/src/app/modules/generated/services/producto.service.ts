import { Injectable, inject } from '@angular/core';
import { GeneratedFacadeBase } from '../../../core/services/generated-facade.base';
import { Producto } from '../models/producto.model';
import { ProductoApiService } from './producto.api.service';

@Injectable({
  providedIn: 'root'
})
export class ProductoService extends GeneratedFacadeBase<Producto, ProductoApiService> {
  protected readonly apiService = inject(ProductoApiService);
}
