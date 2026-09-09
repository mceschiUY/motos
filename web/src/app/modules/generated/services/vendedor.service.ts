import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { GeneratedFacadeBase } from '../../../core/services/generated-facade.base';
import { Vendedor } from '../models/vendedor.model';
import { VendedorApiService } from './vendedor.api.service';

@Injectable({
  providedIn: 'root'
})
export class VendedorService extends GeneratedFacadeBase<Vendedor, VendedorApiService> {
  protected readonly apiService = inject(VendedorApiService);

  /** Vendedor del usuario logueado (para preseleccionar "lo mío"). */
  mio(): Observable<Vendedor> {
    return this.apiService.mio();
  }
}
