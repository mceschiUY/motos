import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

// ═══════════════════════════════════════════════════════════════════════════════
// SENTINEL SERVICE - ZAS.Sentinel API
// Servicio para comunicación con ZAS.Sentinel (motor de evolución)
// ═══════════════════════════════════════════════════════════════════════════════

// ─────────────────────────────────────────────────────────────────────────────
// Interfaces de Request
// ─────────────────────────────────────────────────────────────────────────────

export interface StartEvolutionRequest {
  solicitud: string;
  projectType: 'angular' | 'dotnet' | 'both';
  contexto?: EvolutionContexto;
}

export interface EvolutionContexto {
  rutaActual?: string;
  moduloActual?: string;
  componenteActual?: string;
  entidadRelacionada?: string;
}

export interface AnalyzeRequest {
  solicitud: string;
  contexto?: EvolutionContexto;
}

export interface ExecuteDirectRequest {
  changes: DirectChange[];
}

export interface DirectChange {
  path: string;
  code: string;
  action: 'create' | 'modify' | 'delete';
}

// ─────────────────────────────────────────────────────────────────────────────
// Interfaces de Response
// ─────────────────────────────────────────────────────────────────────────────

export interface StartEvolutionResponse {
  sessionId: string;
  state: EvolutionState;
  copyProjectPath: string;
}

export interface AnalyzeResponse {
  mutationSummary: string;
  structuralImpact: StructuralImpact;
  architectureAudit: ArchitectureAudit;
}

export interface StructuralImpact {
  backendChanges: FileChange[];
  frontendChanges: FileChange[];
}

export interface FileChange {
  path: string;
  action: string;
  code: string;
  language: string;
  description?: string;
}

export interface ArchitectureAudit {
  isCompliant: boolean;
  compliancePercentage: number;
  warnings: string[];
  recommendations: string[];
  message?: string;
}

export interface ExecuteDirectResponse {
  success: boolean;
  archivosCreados: number;
  archivosModificados: number;
  archivosAfectados: string[];
}

export interface EvolutionStatusResponse {
  sessionId?: string;
  state: EvolutionState;
  progressPercent: number;
  detectedChanges: DetectedChange[];
  lastBuild?: BuildResult;
  errorMessage?: string;
  copyProjectPath?: string;
  gitBranchName?: string;
}

export interface DetectedChange {
  path: string;
  changeType: string;
  gitHash?: string;
  author?: string;
  branchName?: string;
}

export interface BuildResult {
  success: boolean;
  duration: string;
}

export interface CommitEvolutionResponse {
  success: boolean;
  filesModified: number;
  filesCreated: number;
  filesDeleted: number;
  affectedFiles: string[];
  gitCommitSha?: string;
  gitBranchName?: string;
}

export interface HealthResponse {
  status: string;
  service: string;
  timestamp: string;
}

export interface CopyHealthResponse {
  status: 'ready' | 'no_directory' | 'no_modules' | 'server_down';
  message: string;
  instructions: string[];

  // Frontend
  copyPath: string;
  copyUrl: string;
  directoryExists: boolean;
  nodeModulesExists: boolean;
  serverRunning: boolean;

  // Backend
  backendCopyPath: string;
  backendCopyUrl: string;
  backendDirectoryExists: boolean;
  backendServerRunning: boolean;
}

// SSE Event types
export interface SseEvent {
  type: string;
  timestamp: number;
  data: any;
}

export interface ProgressEventData {
  phase: string;
  percentage: number;
  message: string;
}

export interface LogEventData {
  level: 'info' | 'warning' | 'error' | 'success';
  message: string;
}

export interface FileChangedEventData {
  path: string;
  action: string;
}

export interface BuildStatusEventData {
  success: boolean;
  error?: string;
}

// ─────────────────────────────────────────────────────────────────────────────
// Git Diff
// ─────────────────────────────────────────────────────────────────────────────

export interface GitDiffResult {
  branchName: string;
  baseBranch: string;
  files: FileDiff[];
  totalAdditions: number;
  totalDeletions: number;
}

export interface FileDiff {
  path: string;
  status: string;
  oldContent: string;
  newContent: string;
  language: string;
  additions: number;
  deletions: number;
}

// ─────────────────────────────────────────────────────────────────────────────
// Evolution Hub - Proposals
// ─────────────────────────────────────────────────────────────────────────────

export interface ProposalsResponse {
  optimization: Proposal[];
  expansion: Proposal[];
  crossCutting: Proposal[];
  summary: ProposalsSummary;
}

export interface Proposal {
  id: string;
  category: string;
  entityName: string | null;
  featureId: string;
  featureName: string;
  description: string;
  businessValue: string;
  priority: 'high' | 'medium' | 'low';
  prompt: string;
  icon: string;
  estimatedImpact: string;
}

export interface ProposalsSummary {
  totalEntities: number;
  totalProposals: number;
  highPriority: number;
  mediumPriority: number;
  lowPriority: number;
  proposalsPerEntity: Record<string, number>;
}

// ─────────────────────────────────────────────────────────────────────────────
// Enums
// ─────────────────────────────────────────────────────────────────────────────

export enum EvolutionState {
  Idle = 'Idle',
  Preparing_Copy = 'Preparing_Copy',
  Muting_Omega = 'Muting_Omega',
  Compiling = 'Compiling',
  Ready_To_Review = 'Ready_To_Review',
  Error = 'Error'
}

// ─────────────────────────────────────────────────────────────────────────────
// Service
// ─────────────────────────────────────────────────────────────────────────────

@Injectable({
  providedIn: 'root'
})
export class SentinelService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = (environment as any).sentinelUrl ?? 'http://localhost:5002';

  /**
   * Verifica que ZAS.Sentinel esté disponible
   */
  health(): Observable<HealthResponse> {
    return this.http.get<HealthResponse>(`${this.baseUrl}/health`);
  }

  /**
   * Verifica el estado del sitio copia (directorio, node_modules, servidor)
   */
  getCopyHealth(): Observable<CopyHealthResponse> {
    return this.http.get<CopyHealthResponse>(`${this.baseUrl}/evolution/copy-health`);
  }

  /**
   * Inicia una nueva evolución: sincroniza la copia del proyecto
   */
  startEvolution(request: StartEvolutionRequest): Observable<StartEvolutionResponse> {
    return this.http.post<StartEvolutionResponse>(`${this.baseUrl}/evolution/start`, request);
  }

  /**
   * Analiza una solicitud con Claude y genera código
   */
  analyze(request: AnalyzeRequest): Observable<AnalyzeResponse> {
    return this.http.post<AnalyzeResponse>(`${this.baseUrl}/evolution/analyze`, request);
  }

  /**
   * Ejecuta cambios directamente en la copia del proyecto
   */
  executeDirect(request: ExecuteDirectRequest): Observable<ExecuteDirectResponse> {
    return this.http.post<ExecuteDirectResponse>(`${this.baseUrl}/evolution/execute-direct`, request);
  }

  /**
   * Obtiene el estado actual de la evolución
   */
  getStatus(): Observable<EvolutionStatusResponse> {
    return this.http.get<EvolutionStatusResponse>(`${this.baseUrl}/evolution/status`);
  }

  /**
   * Confirma los cambios y los aplica al proyecto original
   */
  commit(): Observable<CommitEvolutionResponse> {
    return this.http.post<CommitEvolutionResponse>(`${this.baseUrl}/evolution/commit`, {});
  }

  /**
   * Cancela la evolución actual
   */
  cancel(): Observable<{ success: boolean; message: string }> {
    return this.http.post<{ success: boolean; message: string }>(`${this.baseUrl}/evolution/cancel`, {});
  }

  /**
   * Obtiene el diff git de la evolución actual
   */
  getDiff(): Observable<GitDiffResult> {
    return this.http.get<GitDiffResult>(`${this.baseUrl}/evolution/diff`);
  }

  /**
   * Fuerza un rebuild del proyecto copia
   */
  rebuild(): Observable<{ success: boolean; duration: string; output?: string }> {
    return this.http.post<{ success: boolean; duration: string; output?: string }>(`${this.baseUrl}/evolution/rebuild`, {});
  }

  /**
   * Obtiene propuestas de evolución (gap analysis)
   */
  getProposals(): Observable<ProposalsResponse> {
    return this.http.get<ProposalsResponse>(`${this.baseUrl}/evolution/proposals`);
  }

  /**
   * Invalida la cache de propuestas (forzar re-análisis)
   */
  invalidateProposalsCache(): Observable<{ success: boolean; message: string }> {
    return this.http.post<{ success: boolean; message: string }>(`${this.baseUrl}/evolution/proposals/invalidate`, {});
  }

  /**
   * Conecta al stream SSE para recibir eventos en tiempo real.
   * Retorna un objeto con el EventSource y métodos para manejar eventos.
   */
  connectStream(handlers: {
    onProgress?: (data: ProgressEventData) => void;
    onLog?: (data: LogEventData) => void;
    onFileChanged?: (data: FileChangedEventData) => void;
    onBuildStatus?: (data: BuildStatusEventData) => void;
    onConnected?: () => void;
    onError?: (error: Event) => void;
  }): EventSource {
    const url = `${this.baseUrl}/evolution/stream`;
    const eventSource = new EventSource(url);

    // Evento de conexión
    eventSource.addEventListener('connected', (e: MessageEvent) => {
      console.log('[Sentinel SSE] Conectado');
      handlers.onConnected?.();
    });

    // Evento de progreso
    eventSource.addEventListener('progress', (e: MessageEvent) => {
      try {
        const event: SseEvent = JSON.parse(e.data);
        handlers.onProgress?.(event.data as ProgressEventData);
      } catch (err) {
        console.error('[Sentinel SSE] Error parsing progress:', err);
      }
    });

    // Evento de log
    eventSource.addEventListener('log', (e: MessageEvent) => {
      try {
        const event: SseEvent = JSON.parse(e.data);
        handlers.onLog?.(event.data as LogEventData);
      } catch (err) {
        console.error('[Sentinel SSE] Error parsing log:', err);
      }
    });

    // Evento de archivo modificado
    eventSource.addEventListener('file_changed', (e: MessageEvent) => {
      try {
        const event: SseEvent = JSON.parse(e.data);
        handlers.onFileChanged?.(event.data as FileChangedEventData);
      } catch (err) {
        console.error('[Sentinel SSE] Error parsing file_changed:', err);
      }
    });

    // Evento de estado de build
    eventSource.addEventListener('build_status', (e: MessageEvent) => {
      try {
        const event: SseEvent = JSON.parse(e.data);
        handlers.onBuildStatus?.(event.data as BuildStatusEventData);
      } catch (err) {
        console.error('[Sentinel SSE] Error parsing build_status:', err);
      }
    });

    // Heartbeat (solo log)
    eventSource.addEventListener('heartbeat', () => {
      console.log('[Sentinel SSE] Heartbeat');
    });

    // Error
    eventSource.onerror = (error) => {
      console.error('[Sentinel SSE] Error:', error);
      handlers.onError?.(error);
    };

    return eventSource;
  }
}
