import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { PanelVendedor } from './vendedor-panel.model';

/** Query artesanal read-only del panel del vendedor (HttpClient directo, sin tocar shells). */
@Injectable({ providedIn: 'root' })
export class VendedorPanelService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/artesanal`;

  panel(vendedorId: number): Observable<PanelVendedor> {
    return this.http.get<PanelVendedor>(`${this.base}/vendedor/${vendedorId}/panel`);
  }
}
