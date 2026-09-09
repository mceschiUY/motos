import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { GeneratedApiBase } from '../../../core/services/generated-api.base';
import { Vendedor } from '../models/vendedor.model';

@Injectable({
  providedIn: 'root'
})
export class VendedorApiService extends GeneratedApiBase<Vendedor> {
  protected readonly recurso = 'Vendedor';

  /** El vendedor asociado al usuario logueado (404 si no tiene): GET {apiUrl}/mio. */
  mio(): Observable<Vendedor> {
    return this.http.get<Vendedor>(`${this.apiUrl}/mio`);
  }
}
