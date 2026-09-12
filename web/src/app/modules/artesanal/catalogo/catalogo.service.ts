import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, catchError, map, of, shareReplay } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { DocumentoService } from '../../generated/services/documento.service';
import { CatalogoItem, FichaProducto, PrecioVariante } from './catalogo.model';

/**
 * Queries artesanales read-only del catálogo comercial (contrato artesanal 2026-08-29:
 * datos SOLO por facades existentes o endpoints de lectura; las escrituras siguen yendo
 * por los commands generados).
 */
@Injectable({ providedIn: 'root' })
export class CatalogoService {
  private readonly http = inject(HttpClient);
  private readonly documentos = inject(DocumentoService);
  private readonly url = `${environment.apiUrl}/artesanal/catalogo`;

  /**
   * Caché de portadas por id de documento. Las fotos viajan en base64 dentro del JSON
   * (`/api/Documentos/{id}`) porque `download/{id}` va con [Authorize] y un <img src>
   * no lleva el JWT del interceptor. Sin caché, volver a la grilla las volvería a pedir.
   */
  private readonly portadas = new Map<number, Observable<string | null>>();

  catalogo(marcaId?: number | null, categoriaId?: number | null, q?: string | null): Observable<CatalogoItem[]> {
    let params = new HttpParams();
    if (marcaId != null) params = params.set('marcaId', marcaId);
    if (categoriaId != null) params = params.set('categoriaId', categoriaId);
    if (q) params = params.set('q', q);
    return this.http.get<CatalogoItem[]>(this.url, { params });
  }

  ficha(productoId: number, depositoId?: number | null): Observable<FichaProducto> {
    let params = new HttpParams();
    if (depositoId != null) params = params.set('depositoId', depositoId);
    return this.http.get<FichaProducto>(`${this.url}/${productoId}`, { params });
  }

  historialPrecios(varianteId: number): Observable<PrecioVariante[]> {
    return this.http.get<PrecioVariante[]>(`${environment.apiUrl}/artesanal/variante/${varianteId}/precios`);
  }

  /** Data-URL de una foto de PC_DOCUMENTOS, o null si no se pudo traer. */
  portada(documentoId: number | null): Observable<string | null> {
    if (documentoId == null) { return of(null); }
    const cacheada = this.portadas.get(documentoId);
    if (cacheada) { return cacheada; }
    const pedido = this.documentos.getById(documentoId).pipe(
      map(doc => doc ? `data:${doc.mimeType};base64,${doc.contenido}` : null),
      catchError(() => of(null)),
      shareReplay({ bufferSize: 1, refCount: false }),
    );
    this.portadas.set(documentoId, pedido);
    return pedido;
  }
}
