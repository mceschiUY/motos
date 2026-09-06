import { Injectable, signal, computed, inject } from '@angular/core';
import { SentinelService, ProposalsResponse, Proposal } from './sentinel.service';
import { MutationStateService } from './mutation-state.service';
import { NotificationService } from './notification.service';

// ═══════════════════════════════════════════════════════════════════════════════
// EVOLUTION HUB SERVICE
// Gestiona propuestas de evolución, filtros y materialización
// ═══════════════════════════════════════════════════════════════════════════════

export type HubMode = 'proposals' | 'input';
export type LoadingState = 'idle' | 'loading' | 'loaded' | 'error';

@Injectable({
  providedIn: 'root'
})
export class EvolutionHubService {
  private readonly sentinelService = inject(SentinelService);
  private readonly mutationState = inject(MutationStateService);
  private readonly notification = inject(NotificationService);

  // ═══════════════════════════════════════════════════════════════════════
  // SIGNALS
  // ═══════════════════════════════════════════════════════════════════════

  private readonly _isOpen = signal(false);
  private readonly _mode = signal<HubMode>('proposals');
  private readonly _loadingState = signal<LoadingState>('idle');
  private readonly _proposals = signal<ProposalsResponse | null>(null);
  private readonly _filterEntity = signal<string | null>(null);
  private readonly _filterPriority = signal<string | null>(null);
  private readonly _activeTab = signal<number>(0);
  private readonly _errorMessage = signal<string | null>(null);

  // Public readonly
  readonly isOpen = this._isOpen.asReadonly();
  readonly mode = this._mode.asReadonly();
  readonly loadingState = this._loadingState.asReadonly();
  readonly proposals = this._proposals.asReadonly();
  readonly filterEntity = this._filterEntity.asReadonly();
  readonly filterPriority = this._filterPriority.asReadonly();
  readonly activeTab = this._activeTab.asReadonly();
  readonly errorMessage = this._errorMessage.asReadonly();

  // ═══════════════════════════════════════════════════════════════════════
  // COMPUTED
  // ═══════════════════════════════════════════════════════════════════════

  readonly summary = computed(() => this._proposals()?.summary ?? null);

  readonly entityNames = computed(() => {
    const props = this._proposals();
    if (!props) return [];
    const entities = new Set<string>();
    [...props.optimization, ...props.expansion].forEach(p => {
      if (p.entityName) entities.add(p.entityName);
    });
    return Array.from(entities).sort();
  });

  readonly filteredOptimization = computed(() =>
    this.applyFilters(this._proposals()?.optimization ?? [])
  );

  readonly filteredExpansion = computed(() =>
    this.applyFilters(this._proposals()?.expansion ?? [])
  );

  readonly filteredCrossCutting = computed(() => {
    const proposals = this._proposals()?.crossCutting ?? [];
    const priority = this._filterPriority();
    if (!priority) return proposals;
    return proposals.filter(p => p.priority === priority);
  });

  readonly hasProposals = computed(() => {
    const s = this.summary();
    return s ? s.totalProposals > 0 : false;
  });

  readonly totalFilteredCount = computed(() =>
    this.filteredOptimization().length +
    this.filteredExpansion().length +
    this.filteredCrossCutting().length
  );

  // ═══════════════════════════════════════════════════════════════════════
  // ACTIONS
  // ═══════════════════════════════════════════════════════════════════════

  toggle(): void {
    if (this._isOpen()) {
      this.close();
    } else {
      this.open();
    }
  }

  open(): void {
    this._isOpen.set(true);
    if (this._loadingState() === 'idle' || this._loadingState() === 'error') {
      this.loadProposals();
    }
  }

  close(): void {
    this._isOpen.set(false);
  }

  setActiveTab(index: number): void {
    this._activeTab.set(index);
  }

  setFilterEntity(entity: string | null): void {
    this._filterEntity.set(entity);
  }

  setFilterPriority(priority: string | null): void {
    this._filterPriority.set(priority);
  }

  clearFilters(): void {
    this._filterEntity.set(null);
    this._filterPriority.set(null);
  }

  // ═══════════════════════════════════════════════════════════════════════
  // DATA LOADING
  // ═══════════════════════════════════════════════════════════════════════

  loadProposals(): void {
    this._loadingState.set('loading');
    this._errorMessage.set(null);

    this.sentinelService.getProposals().subscribe({
      next: (response) => {
        this._proposals.set(response);
        this._loadingState.set('loaded');
      },
      error: (err) => {
        console.error('[EvolutionHub] Error cargando propuestas:', err);
        this._errorMessage.set(err?.error?.message || err?.message || 'Error al cargar propuestas');
        this._loadingState.set('error');
      }
    });
  }

  refreshProposals(): void {
    this.sentinelService.invalidateProposalsCache().subscribe({
      next: () => this.loadProposals(),
      error: () => this.loadProposals()
    });
  }

  // ═══════════════════════════════════════════════════════════════════════
  // MATERIALIZE - Conecta con Mutation Sidebar
  // ═══════════════════════════════════════════════════════════════════════

  materializeProposal(proposal: Proposal): void {
    this.close();
    this.mutationState.setSolicitud(proposal.prompt);
    this.mutationState.open();
    this.notification.info(`Propuesta "${proposal.featureName}" lista para ejecutar`);
  }

  // ═══════════════════════════════════════════════════════════════════════
  // PRIVATE
  // ═══════════════════════════════════════════════════════════════════════

  private applyFilters(proposals: Proposal[]): Proposal[] {
    let filtered = proposals;
    const entity = this._filterEntity();
    const priority = this._filterPriority();

    if (entity) {
      filtered = filtered.filter(p => p.entityName === entity);
    }
    if (priority) {
      filtered = filtered.filter(p => p.priority === priority);
    }
    return filtered;
  }
}
