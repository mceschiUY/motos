import { Injectable, signal, computed, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { Proposal } from '../../../core/services/sentinel.service';

// ═══════════════════════════════════════════════════════════════════════════════
// EVOLUTION BOARD SERVICE
// Gestiona el estado del Kanban y la comunicación con el backend
// ═══════════════════════════════════════════════════════════════════════════════

export type EvolutionItemEstado = 'Backlog' | 'Approved' | 'Building' | 'Review' | 'Deployed' | 'Rejected' | 'Failed';

export interface EvolutionItem {
  id: number;
  titulo: string;
  descripcion: string;
  businessValue: string;
  estado: string;
  prioridad: string;
  entidadRelacionada: string | null;
  categoria: string;
  promptMutation: string;
  featureId: string | null;
  mutacionId: number | null;
  icon: string;
  orden: number;
  fechaCreacion: string;
  fechaAprobacion: string | null;
  fechaInicioBuild: string | null;
  fechaDespliegue: string | null;
  errorMessage: string | null;
}

export interface BoardData {
  backlog: EvolutionItem[];
  approved: EvolutionItem[];
  building: EvolutionItem[];
  review: EvolutionItem[];
  deployed: EvolutionItem[];
  failed: EvolutionItem[];
  rejected: EvolutionItem[];
}

export interface BoardColumn {
  id: EvolutionItemEstado;
  label: string;
  icon: string;
  color: string;
  items: EvolutionItem[];
}

@Injectable({
  providedIn: 'root'
})
export class EvolutionBoardService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = (environment as any).apiUrl ?? 'http://localhost:5000';

  // ═══════════════════════════════════════════════════════════════════════
  // SIGNALS
  // ═══════════════════════════════════════════════════════════════════════

  private readonly _boardData = signal<BoardData | null>(null);
  private readonly _loading = signal(false);
  private readonly _error = signal<string | null>(null);

  readonly boardData = this._boardData.asReadonly();
  readonly loading = this._loading.asReadonly();
  readonly error = this._error.asReadonly();

  readonly columns = computed<BoardColumn[]>(() => {
    const data = this._boardData();
    if (!data) return this.emptyColumns();

    // Colores de DATOS (viajan al template como [style]): valores de la paleta CESKIA
    // en duro — azul #009FE3, amarillo #ffcc44, verde #1ebe57, gris = text-secondary dark.
    return [
      { id: 'Backlog', label: 'Backlog', icon: 'inbox', color: '#7A8FA6', items: data.backlog },
      { id: 'Approved', label: 'Aprobado', icon: 'check_circle', color: '#009FE3', items: data.approved },
      { id: 'Building', label: 'Construyendo', icon: 'construction', color: '#009FE3', items: data.building },
      { id: 'Review', label: 'Revisión', icon: 'rate_review', color: '#ffcc44', items: data.review },
      { id: 'Deployed', label: 'Desplegado', icon: 'rocket_launch', color: '#1ebe57', items: data.deployed },
    ];
  });

  readonly failedItems = computed(() => this._boardData()?.failed ?? []);
  readonly rejectedItems = computed(() => this._boardData()?.rejected ?? []);

  readonly totalItems = computed(() => {
    const data = this._boardData();
    if (!data) return 0;
    return data.backlog.length + data.approved.length + data.building.length +
           data.review.length + data.deployed.length + data.failed.length + data.rejected.length;
  });

  // ═══════════════════════════════════════════════════════════════════════
  // API
  // ═══════════════════════════════════════════════════════════════════════

  loadBoard(): void {
    this._loading.set(true);
    this._error.set(null);

    this.http.get<BoardData>(`${this.baseUrl}/api/evolution/board`).subscribe({
      next: (data) => {
        this._boardData.set(data);
        this._loading.set(false);
      },
      error: (err) => {
        this._error.set(err?.error?.message || err?.message || 'Error al cargar el board');
        this._loading.set(false);
      }
    });
  }

  moveItem(itemId: number, newEstado: EvolutionItemEstado, errorMessage?: string): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/api/evolution/items/${itemId}/estado`, {
      estado: newEstado,
      errorMessage
    });
  }

  reorderItem(itemId: number, newOrden: number): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/api/evolution/items/${itemId}/reorder`, {
      orden: newOrden
    });
  }

  createItem(item: any): Observable<number> {
    return this.http.post<number>(`${this.baseUrl}/api/evolution/items`, item);
  }

  importProposals(proposals: Proposal[]): Observable<{ imported: number; skipped: number }> {
    const items = proposals.map(p => ({
      featureId: p.featureId,
      featureName: p.featureName,
      description: p.description,
      businessValue: p.businessValue,
      priority: p.priority,
      entityName: p.entityName,
      category: p.category,
      prompt: p.prompt,
      icon: p.icon
    }));

    return this.http.post<{ imported: number; skipped: number }>(
      `${this.baseUrl}/api/evolution/items/import`,
      { proposals: items }
    );
  }

  deleteItem(itemId: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/api/evolution/items/${itemId}`);
  }

  // ═══════════════════════════════════════════════════════════════════════
  // HELPERS
  // ═══════════════════════════════════════════════════════════════════════

  private emptyColumns(): BoardColumn[] {
    return [
      { id: 'Backlog', label: 'Backlog', icon: 'inbox', color: '#7A8FA6', items: [] },
      { id: 'Approved', label: 'Aprobado', icon: 'check_circle', color: '#009FE3', items: [] },
      { id: 'Building', label: 'Construyendo', icon: 'construction', color: '#009FE3', items: [] },
      { id: 'Review', label: 'Revisión', icon: 'rate_review', color: '#ffcc44', items: [] },
      { id: 'Deployed', label: 'Desplegado', icon: 'rocket_launch', color: '#1ebe57', items: [] },
    ];
  }
}
