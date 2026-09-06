import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of, catchError, tap } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { Conceptoexpensa } from '../models/conceptoexpensa.model';
import { ConceptoexpensaApiService } from './conceptoexpensa.api.service';

interface EntityStatus {
  stageName: string;
  currentStage: number;
}

@Injectable({
  providedIn: 'root'
})
export class ConceptoexpensaService {
  private readonly http = inject(HttpClient);
  private readonly apiService = inject(ConceptoexpensaApiService);

  // Siempre usar API real
  private readonly _useApi = signal<boolean>(true);
  readonly useApi = this._useApi.asReadonly();

  // Signal para el estado actual
  private readonly _currentStage = signal<string>('SYNCED');
  readonly currentStage = this._currentStage.asReadonly();

  // ═══════════════════════════════════════════════════════════════════════════════
  // MÉTODOS CRUD - Delegan al servicio API
  // ═══════════════════════════════════════════════════════════════════════════════

  getAll(): Observable<Conceptoexpensa[]> {
    return this.apiService.getAll();
  }

  getById(id: string): Observable<Conceptoexpensa | undefined> {
    return this.apiService.getById(id);
  }

  create(data: Omit<Conceptoexpensa, 'id'>): Observable<Conceptoexpensa> {
    return this.apiService.create(data);
  }

  update(id: string, data: Partial<Conceptoexpensa>): Observable<Conceptoexpensa | undefined> {
    return this.apiService.update(id, data);
  }

  delete(id: string): Observable<boolean | void> {
    return this.apiService.delete(id);
  }

  deleteMultiple(ids: (string | number)[]): Observable<boolean | void> {
    return this.apiService.deleteMultiple(ids);
  }

  // ═══════════════════════════════════════════════════════════════════════════════
  // MÉTODOS DE NAVEGACIÓN - Obtener por FK
  // ═══════════════════════════════════════════════════════════════════════════════
}
