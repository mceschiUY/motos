import { signal } from '@angular/core';
import { Observable } from 'rxjs';
import { GeneratedApiBase } from './generated-api.base';

// ─── Base de los facade services generados (Fase A1.2) ──────────────────────
// La facade delega el CRUD al API service; el generador emite por entidad un
// shell que inyecta SU api y expone los wrappers de FK/acciones con los mismos
// nombres públicos (getByXxxId, pasarAXxx). Las firmas se conservan tal cual
// las estampaba la plantilla (getById → T | undefined, delete → boolean | void).

export abstract class GeneratedFacadeBase<
  T extends { id: number | string },
  TApi extends GeneratedApiBase<T>,
> {
  protected abstract readonly apiService: TApi;

  // Siempre usar API real
  private readonly _useApi = signal<boolean>(true);
  readonly useApi = this._useApi.asReadonly();

  // Signal para el estado actual
  private readonly _currentStage = signal<string>('SYNCED');
  readonly currentStage = this._currentStage.asReadonly();

  getAll(): Observable<T[]> {
    return this.apiService.getAll();
  }

  getById(id: number | string): Observable<T | undefined> {
    return this.apiService.getById(id);
  }

  create(data: Omit<T, 'id'>): Observable<T> {
    return this.apiService.create(data);
  }

  update(id: number | string, data: Partial<T>): Observable<T | undefined> {
    return this.apiService.update(id, data);
  }

  delete(id: number | string): Observable<boolean | void> {
    return this.apiService.delete(id);
  }

  deleteMultiple(ids: (string | number)[]): Observable<boolean | void> {
    return this.apiService.deleteMultiple(ids);
  }
}
