import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { GeneratedApiBase } from '../../../core/services/generated-api.base';
import { Meta } from '../models/meta.model';

@Injectable({
  providedIn: 'root'
})
export class MetaApiService extends GeneratedApiBase<Meta> {
  protected readonly recurso = 'Meta';

  getByVendedorId(vendedorId: number): Observable<Meta[]> {
    return this.byFk('vendedor', vendedorId);
  }
}
