import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { GeneratedFacadeBase } from '../../../core/services/generated-facade.base';
import { Meta } from '../models/meta.model';
import { MetaApiService } from './meta.api.service';

@Injectable({
  providedIn: 'root'
})
export class MetaService extends GeneratedFacadeBase<Meta, MetaApiService> {
  protected readonly apiService = inject(MetaApiService);

  getByVendedorId(vendedorId: number): Observable<Meta[]> {
    return this.apiService.getByVendedorId(vendedorId);
  }
}
