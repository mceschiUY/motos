import { inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

// ─── Base de los API services generados (Fase A1.2) ─────────────────────────
// Los 6 métodos CRUD son idénticos en toda entidad: viven acá UNA vez. El
// generador emite por entidad un shell que declara `recurso` y expone los
// wrappers de FK (getByXxxId → byFk) y de ciclo (pasarAXxx → accion) con los
// MISMOS nombres públicos de siempre — los componentes no se enteran.

export abstract class GeneratedApiBase<T extends { id: number | string }> {
  protected readonly http = inject(HttpClient);

  /** Nombre del recurso en la API (segmento de ruta): 'Cliente', 'Oportunidad'. */
  protected abstract readonly recurso: string;

  protected get apiUrl(): string {
    return `${environment.apiUrl}/${this.recurso}`;
  }

  getAll(): Observable<T[]> {
    return this.http.get<T[]>(this.apiUrl);
  }

  getById(id: number | string): Observable<T> {
    return this.http.get<T>(`${this.apiUrl}/${id}`);
  }

  create(data: Omit<T, 'id'>): Observable<T> {
    return this.http.post<T>(this.apiUrl, data);
  }

  update(id: number | string, data: Partial<T>): Observable<T> {
    return this.http.put<T>(`${this.apiUrl}/${id}`, data);
  }

  delete(id: number | string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  deleteMultiple(ids: (string | number)[]): Observable<void> {
    return this.http.request<void>('delete', `${this.apiUrl}/bulk`, { body: ids });
  }

  /** Navegación por FK: GET {apiUrl}/by-{fk}/{id}. */
  protected byFk(fkLower: string, fkId: number): Observable<T[]> {
    return this.http.get<T[]>(`${this.apiUrl}/by-${fkLower}/${fkId}`);
  }

  /** Acción de ciclo (capability no-CRUD): POST {apiUrl}/{id}/{kebab}. */
  protected accion(id: string | number, kebab: string): Observable<boolean> {
    return this.http.post<boolean>(`${this.apiUrl}/${id}/${kebab}`, {});
  }
}
