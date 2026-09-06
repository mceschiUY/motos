import { Injectable, signal, computed, inject } from '@angular/core';
import {
  MutationService,
  MutacionResponse,
  EjecucionResultado,
  MutacionHistorial
} from './mutation.service';
import {
  SentinelService,
  EvolutionState,
  AnalyzeResponse,
  EvolutionContexto,
  LogEventData
} from './sentinel.service';
import { NotificationService } from './notification.service';
import { firstValueFrom } from 'rxjs';

// ═══════════════════════════════════════════════════════════════════════════════
// MUTATION STATE SERVICE - Integrado con ZAS.Sentinel
// Maneja el estado de mutaciones usando el motor de evolución
// Flujo: input → syncing → analyzing → applying → building → done
// ═══════════════════════════════════════════════════════════════════════════════

export type MutationStep = 'input' | 'executing' | 'done' | 'error';
export type StreamingPhase = 'idle' | 'syncing' | 'analyzing' | 'applying' | 'building' | 'complete' | 'error';

@Injectable({
  providedIn: 'root'
})
export class MutationStateService {
  private readonly mutationService = inject(MutationService);
  private readonly sentinelService = inject(SentinelService);
  private readonly notification = inject(NotificationService);

  // SSE connection
  private eventSource: EventSource | null = null;

  // Polling interval (fallback)
  private statusPollInterval: ReturnType<typeof setInterval> | null = null;

  // ═══════════════════════════════════════════════════════════════════════
  // SIGNALS - Estado unificado
  // ═══════════════════════════════════════════════════════════════════════

  private readonly _isOpen = signal(false);
  private readonly _step = signal<MutationStep>('input');
  private readonly _solicitud = signal('');
  private readonly _error = signal<string | null>(null);
  private readonly _ejecutado = signal<EjecucionResultado | null>(null);
  private readonly _response = signal<MutacionResponse | null>(null);
  private readonly _historial = signal<MutacionHistorial[]>([]);
  private readonly _sessionId = signal<string | null>(null);

  // Streaming - un solo objeto para reducir signals
  private readonly _streaming = signal({
    phase: 'idle' as StreamingPhase,
    progress: 0,
    message: ''
  });

  private readonly _appliedChanges = signal<any[]>([]);
  private readonly _analyzeResponse = signal<AnalyzeResponse | null>(null);
  private readonly _logs = signal<LogEventData[]>([]);

  // Public readonly
  readonly isOpen = this._isOpen.asReadonly();
  readonly step = this._step.asReadonly();
  readonly solicitud = this._solicitud.asReadonly();
  readonly error = this._error.asReadonly();
  readonly ejecutado = this._ejecutado.asReadonly();
  readonly response = this._response.asReadonly();
  readonly historial = this._historial.asReadonly();
  readonly sessionId = this._sessionId.asReadonly();
  readonly analyzeResponse = this._analyzeResponse.asReadonly();
  readonly logs = this._logs.asReadonly();

  // Computed from streaming
  readonly streamingPhase = computed(() => this._streaming().phase);
  readonly streamingProgress = computed(() => this._streaming().progress);
  readonly streamingMessage = computed(() => this._streaming().message);
  readonly isExecuting = computed(() => this._step() === 'executing');
  readonly isDone = computed(() => this._step() === 'done');

  // ═══════════════════════════════════════════════════════════════════════
  // ACTIONS
  // ═══════════════════════════════════════════════════════════════════════

  toggle(): void {
    this._isOpen.update(v => !v);
  }

  open(): void {
    this._isOpen.set(true);
  }

  close(): void {
    this._isOpen.set(false);
    this.stopPolling();
    this.disconnectSse();
    this.reset();
  }

  reset(): void {
    this._step.set('input');
    this._solicitud.set('');
    this._response.set(null);
    this._ejecutado.set(null);
    this._error.set(null);
    this._sessionId.set(null);
    this._analyzeResponse.set(null);
    this._appliedChanges.set([]);
    this._logs.set([]);
    this._streaming.set({ phase: 'idle', progress: 0, message: '' });
  }

  private connectSse(): void {
    this.disconnectSse();

    this.eventSource = this.sentinelService.connectStream({
      onProgress: (data) => {
        this._streaming.set({
          phase: data.phase as StreamingPhase,
          progress: data.percentage,
          message: data.message
        });
      },
      onLog: (data) => {
        this._logs.update(logs => [...logs.slice(-50), data]); // Mantener últimos 50 logs
      },
      onFileChanged: (data) => {
        console.log('[ZAS SSE] File changed:', data.path, data.action);
      },
      onBuildStatus: (data) => {
        if (data.success) {
          this._streaming.set({ phase: 'complete', progress: 100, message: 'Build exitoso' });
        } else {
          this._error.set(data.error || 'Build falló');
          this._step.set('error');
        }
      },
      onConnected: () => {
        console.log('[ZAS SSE] Conectado a Sentinel');
      },
      onError: () => {
        console.warn('[ZAS SSE] Error de conexión, usando fallback polling');
      }
    });
  }

  private disconnectSse(): void {
    if (this.eventSource) {
      this.eventSource.close();
      this.eventSource = null;
    }
  }

  setSolicitud(value: string): void {
    this._solicitud.set(value);
  }

  // ═══════════════════════════════════════════════════════════════════════
  // SENTINEL EVOLUTION FLOW
  // ═══════════════════════════════════════════════════════════════════════

  async analyzeWithStreaming(contexto?: EvolutionContexto): Promise<void> {
    const solicitud = this._solicitud();
    if (!solicitud.trim()) {
      this._error.set('Por favor, describe el cambio que deseas realizar');
      return;
    }

    this._step.set('executing');
    this._error.set(null);
    this._logs.set([]);

    // Conectar al SSE para recibir actualizaciones en tiempo real
    this.connectSse();

    try {
      // Paso 1: Sincronizar copia del proyecto
      this._streaming.set({ phase: 'syncing', progress: 10, message: 'Sincronizando proyecto...' });
      console.log('[ZAS] Iniciando evolución...');

      const startResult = await firstValueFrom(this.sentinelService.startEvolution({
        solicitud,
        projectType: 'angular',
        contexto
      }));

      this._sessionId.set(startResult.sessionId);
      console.log('[ZAS] Evolución iniciada:', startResult.sessionId);

      // Paso 2: Analizar con Claude
      this._streaming.set({ phase: 'analyzing', progress: 30, message: 'Claude está analizando...' });
      console.log('[ZAS] Analizando con Claude...');

      const analyzeResult = await firstValueFrom(this.sentinelService.analyze({
        solicitud,
        contexto
      }));

      this._analyzeResponse.set(analyzeResult);
      console.log('[ZAS] Análisis completado:', analyzeResult.mutationSummary);

      // Paso 3: Aplicar cambios en la copia
      const allChanges = [
        ...(analyzeResult.structuralImpact.frontendChanges || []),
        ...(analyzeResult.structuralImpact.backendChanges || [])
      ];

      if (allChanges.length === 0) {
        throw new Error('Claude no generó ningún cambio');
      }

      this._streaming.set({ phase: 'applying', progress: 60, message: `Aplicando ${allChanges.length} archivo(s)...` });
      console.log('[ZAS] Aplicando cambios:', allChanges.map(c => c.path));

      const executeResult = await firstValueFrom(this.sentinelService.executeDirect({
        changes: allChanges.map(c => ({
          path: c.path,
          code: c.code,
          action: c.action as 'create' | 'modify' | 'delete'
        }))
      }));

      this._appliedChanges.set(allChanges);
      console.log('[ZAS] Cambios aplicados:', executeResult);

      // Paso 4: Esperar build (polling)
      this._streaming.set({ phase: 'building', progress: 80, message: 'Compilando proyecto...' });
      await this.waitForBuild();

      // Éxito
      this._ejecutado.set({
        success: true,
        mutacionId: 0,
        archivosCreados: executeResult.archivosCreados,
        archivosModificados: executeResult.archivosModificados,
        archivosEliminados: 0,
        archivosAfectados: executeResult.archivosAfectados
      });

      // Mapear FileChange a CambioArchivo con el tipo de action correcto
      const mapChanges = (changes: any[]) => changes.map(c => ({
        ...c,
        action: c.action as 'create' | 'modify' | 'delete'
      }));

      this._response.set({
        mutacionId: 0,
        mutationSummary: analyzeResult.mutationSummary,
        previewHtml: '',
        structuralImpact: {
          frontendChanges: mapChanges(analyzeResult.structuralImpact.frontendChanges || []),
          backendChanges: mapChanges(analyzeResult.structuralImpact.backendChanges || [])
        },
        architectureAudit: analyzeResult.architectureAudit
      });

      this._step.set('done');
      this._streaming.set({
        phase: 'complete',
        progress: 100,
        message: `${executeResult.archivosModificados + executeResult.archivosCreados} archivo(s) modificados`
      });

      this.notification.success('Cambios aplicados exitosamente');

    } catch (err: any) {
      console.error('[ZAS] Error:', err);
      this._error.set(err?.message || err?.error?.message || 'Error durante la evolución');
      this._step.set('error');
      this._streaming.set({ phase: 'error', progress: 0, message: err?.message || 'Error' });
    } finally {
      // Mantener SSE conectado para seguir recibiendo eventos
      // Se desconectará al cerrar el modal o al hacer reset
    }
  }

  private async waitForBuild(): Promise<void> {
    const maxAttempts = 60; // 60 segundos máximo
    let attempts = 0;

    return new Promise((resolve, reject) => {
      this.statusPollInterval = setInterval(async () => {
        attempts++;

        try {
          const status = await firstValueFrom(this.sentinelService.getStatus());
          console.log('[ZAS] Status:', status.state, status.progressPercent);

          if (status.state === EvolutionState.Ready_To_Review) {
            this.stopPolling();
            resolve();
          } else if (status.state === EvolutionState.Error) {
            this.stopPolling();
            reject(new Error(status.errorMessage || 'Build falló'));
          } else if (attempts >= maxAttempts) {
            this.stopPolling();
            // Timeout pero no fallar - puede que el build no esté configurado
            console.warn('[ZAS] Timeout esperando build, continuando...');
            resolve();
          }
        } catch (err) {
          console.warn('[ZAS] Error polling status:', err);
          // No fallar por errores de polling
          if (attempts >= maxAttempts) {
            this.stopPolling();
            resolve();
          }
        }
      }, 1000);
    });
  }

  private stopPolling(): void {
    if (this.statusPollInterval) {
      clearInterval(this.statusPollInterval);
      this.statusPollInterval = null;
    }
  }

  // ═══════════════════════════════════════════════════════════════════════
  // COMMIT & CANCEL
  // ═══════════════════════════════════════════════════════════════════════

  async commitChanges(): Promise<void> {
    try {
      this._streaming.set({ phase: 'applying', progress: 50, message: 'Aplicando al proyecto original...' });

      const result = await firstValueFrom(this.sentinelService.commit());

      if (result.success) {
        this.notification.success(`Cambios aplicados: ${result.filesModified + result.filesCreated} archivo(s)`);
        // Invalidar cache de propuestas para que el Evolution Hub se actualice
        this.sentinelService.invalidateProposalsCache().subscribe();
        this.reset();
      } else {
        throw new Error('Error al aplicar cambios');
      }
    } catch (err: any) {
      this.notification.error(err?.message || 'Error al aplicar cambios');
    }
  }

  async cancelEvolution(): Promise<void> {
    try {
      await firstValueFrom(this.sentinelService.cancel());
      this.notification.info('Evolución cancelada');
      this.reset();
    } catch (err: any) {
      console.error('[ZAS] Error cancelando:', err);
    }
  }

  // ═══════════════════════════════════════════════════════════════════════
  // REVERT (usando ApiMotos para git checkout)
  // ═══════════════════════════════════════════════════════════════════════

  async revertChanges(): Promise<void> {
    try {
      await firstValueFrom(this.sentinelService.cancel());

      // Fallback: revertir archivos aplicados al original via ApiMotos git checkout
      const changes = this._appliedChanges();
      if (changes.length > 0) {
        const paths = changes.map((c: any) => c.path);
        try {
          await firstValueFrom(this.mutationService.revertFiles(paths));
        } catch {
          // revertFiles es un safety net; no fallar si ApiMotos no responde
        }
      }

      this.notification.success('Cambios revertidos');
      this._appliedChanges.set([]);
      this.reset();
    } catch (err: any) {
      this.notification.error(err?.message || 'Error al revertir');
    }
  }

  // ═══════════════════════════════════════════════════════════════════════
  // HISTORIAL (usando ApiMotos)
  // ═══════════════════════════════════════════════════════════════════════

  async loadHistorial(): Promise<void> {
    try {
      const historial = await firstValueFrom(this.mutationService.getRecientes(5));
      if (historial) {
        this._historial.set(historial);
      }
    } catch (err) {
      console.error('Error cargando historial:', err);
    }
  }

  newMutation(): void {
    this.stopPolling();
    this.disconnectSse();
    this.reset();
  }

  cancel(): void {
    this.stopPolling();
    this.disconnectSse();
    this.cancelEvolution();
  }
}
