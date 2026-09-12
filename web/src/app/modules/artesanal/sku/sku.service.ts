import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { CatalogoService } from '../catalogo/catalogo.service';
import { PrecioVariante } from '../catalogo/catalogo.model';
import { FichaSku } from './sku.model';

/**
 * Query artesanal read-only de la ficha de SKU (contrato artesanal 2026-08-29: datos SOLO
 * por endpoints de lectura; las escrituras siguen yendo por los commands generados).
 * La portada y el historial de precios se reutilizan de CatalogoService (mismo endpoint y caché).
 */
@Injectable({ providedIn: 'root' })
export class SkuService {
  private readonly http = inject(HttpClient);
  private readonly catalogo = inject(CatalogoService);
  private readonly url = `${environment.apiUrl}/artesanal/sku`;

  ficha(varianteId: number): Observable<FichaSku> {
    return this.http.get<FichaSku>(`${this.url}/${varianteId}`);
  }

  historialPrecios(varianteId: number): Observable<PrecioVariante[]> {
    return this.catalogo.historialPrecios(varianteId);
  }

  /** Data-URL de la portada del producto, o null si no se pudo traer. */
  portada(documentoId: number | null): Observable<string | null> {
    return this.catalogo.portada(documentoId);
  }
}
