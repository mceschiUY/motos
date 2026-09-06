import { Injectable, signal, OnDestroy } from '@angular/core';
import { GeneratedFileState, MutationStreamEvent } from '../models/mutation-stream.model';

// ═══════════════════════════════════════════════════════════════════════════════
// MUTATION CHANNEL SERVICE
// Comunicación entre ventanas usando BroadcastChannel API
// Permite enviar eventos de streaming desde la ventana principal a la ventana de mutation
// ═══════════════════════════════════════════════════════════════════════════════

export interface MutationChannelMessage {
  type: 'stream_event' | 'open_request' | 'close_request' | 'state_sync' | 'window_ready';
  payload?: any;
}

export interface MutationWindowState {
  isStreaming: boolean;
  phase: string;
  progress: number;
  message: string;
  files: Map<string, GeneratedFileState>;
  previewHtml: string;
  solicitud: string;
  error: string | null;
  step: string;
  response: any | null;
}

const CHANNEL_NAME = 'zas-mutation-channel';

@Injectable({
  providedIn: 'root'
})
export class MutationChannelService implements OnDestroy {
  private channel: BroadcastChannel | null = null;
  private windowRef: Window | null = null;

  // Señales para el estado recibido (usado en la ventana secundaria)
  readonly isStreaming = signal(false);
  readonly streamingPhase = signal('idle');
  readonly streamingProgress = signal(0);
  readonly streamingMessage = signal('');
  readonly streamingFiles = signal<Map<string, GeneratedFileState>>(new Map());
  readonly streamingPreviewHtml = signal('');
  readonly solicitud = signal('');
  readonly error = signal<string | null>(null);
  readonly step = signal('input');
  readonly response = signal<any | null>(null);
  readonly windowReady = signal(false);

  private messageHandler: ((event: MessageEvent) => void) | null = null;

  constructor() {
    this.initChannel();
  }

  ngOnDestroy(): void {
    this.closeChannel();
  }

  // ═══════════════════════════════════════════════════════════════════════
  // CHANNEL MANAGEMENT
  // ═══════════════════════════════════════════════════════════════════════

  private initChannel(): void {
    if (typeof BroadcastChannel === 'undefined') {
      console.warn('BroadcastChannel not supported in this browser');
      return;
    }

    this.channel = new BroadcastChannel(CHANNEL_NAME);
    this.messageHandler = (event: MessageEvent<MutationChannelMessage>) => {
      this.handleMessage(event.data);
    };
    this.channel.addEventListener('message', this.messageHandler);
  }

  private closeChannel(): void {
    if (this.channel && this.messageHandler) {
      this.channel.removeEventListener('message', this.messageHandler);
      this.channel.close();
      this.channel = null;
    }
  }

  // ═══════════════════════════════════════════════════════════════════════
  // MESSAGE HANDLING
  // ═══════════════════════════════════════════════════════════════════════

  private handleMessage(message: MutationChannelMessage): void {
    switch (message.type) {
      case 'stream_event':
        this.handleStreamEvent(message.payload);
        break;

      case 'state_sync':
        this.syncState(message.payload);
        break;

      case 'window_ready':
        this.windowReady.set(true);
        break;

      case 'close_request':
        // La ventana principal solicita cerrar
        break;
    }
  }

  private handleStreamEvent(event: MutationStreamEvent): void {
    switch (event.type) {
      case 'start':
        this.isStreaming.set(true);
        this.streamingMessage.set(event.message || '');
        break;

      case 'progress':
        this.streamingPhase.set(event.phase || 'idle');
        this.streamingProgress.set(event.percentage || 0);
        this.streamingMessage.set(event.message || '');
        break;

      case 'file_start':
        const newFile: GeneratedFileState = {
          fileId: event.fileId!,
          fileName: event.fileName!,
          filePath: event.filePath!,
          language: event.language!,
          layer: event.layer!,
          content: '',
          isComplete: false
        };
        const filesWithNew = new Map(this.streamingFiles());
        filesWithNew.set(event.fileId!, newFile);
        this.streamingFiles.set(filesWithNew);
        break;

      case 'file_chunk':
        const currentFiles = this.streamingFiles();
        const existingFile = currentFiles.get(event.fileId!);
        if (existingFile) {
          const updatedFile = {
            ...existingFile,
            content: existingFile.content + (event.content || ''),
            isComplete: event.isComplete || false
          };
          const filesWithChunk = new Map(currentFiles);
          filesWithChunk.set(event.fileId!, updatedFile);
          this.streamingFiles.set(filesWithChunk);
        }
        break;

      case 'file_complete':
        const files = this.streamingFiles();
        const fileToComplete = files.get(event.fileId!);
        if (fileToComplete) {
          const completedFile = {
            ...fileToComplete,
            isComplete: true,
            action: event.action,
            totalLines: event.totalLines
          };
          const filesWithComplete = new Map(files);
          filesWithComplete.set(event.fileId!, completedFile);
          this.streamingFiles.set(filesWithComplete);
        }
        break;

      case 'preview_html':
        this.streamingPreviewHtml.set(event.html || '');
        break;

      case 'analysis_complete':
        this.isStreaming.set(false);
        this.streamingProgress.set(100);
        this.step.set('preview');
        this.response.set(event);
        break;

      case 'error':
        this.error.set(event.message || 'Error desconocido');
        this.step.set('error');
        this.isStreaming.set(false);
        break;
    }
  }

  private syncState(state: MutationWindowState): void {
    this.isStreaming.set(state.isStreaming);
    this.streamingPhase.set(state.phase);
    this.streamingProgress.set(state.progress);
    this.streamingMessage.set(state.message);

    // Convertir array de files a Map
    if (state.files instanceof Map) {
      this.streamingFiles.set(state.files);
    } else if (Array.isArray(state.files)) {
      const filesMap = new Map<string, GeneratedFileState>();
      (state.files as any[]).forEach(([key, value]) => {
        filesMap.set(key, value);
      });
      this.streamingFiles.set(filesMap);
    }

    this.streamingPreviewHtml.set(state.previewHtml);
    this.solicitud.set(state.solicitud);
    this.error.set(state.error);
    this.step.set(state.step);
    this.response.set(state.response);
  }

  // ═══════════════════════════════════════════════════════════════════════
  // PUBLIC METHODS - Para enviar mensajes
  // ═══════════════════════════════════════════════════════════════════════

  /**
   * Envía un evento de streaming a todas las ventanas
   */
  sendStreamEvent(event: MutationStreamEvent): void {
    this.postMessage({
      type: 'stream_event',
      payload: event
    });
  }

  /**
   * Sincroniza el estado completo con las ventanas
   */
  syncFullState(state: MutationWindowState): void {
    // Convertir Map a array para serialización
    const serializedState = {
      ...state,
      files: Array.from(state.files.entries())
    };
    this.postMessage({
      type: 'state_sync',
      payload: serializedState
    });
  }

  /**
   * Notifica que la ventana está lista
   */
  notifyWindowReady(): void {
    this.postMessage({ type: 'window_ready' });
  }

  /**
   * Solicita cerrar la ventana secundaria
   */
  requestClose(): void {
    this.postMessage({ type: 'close_request' });
  }

  private postMessage(message: MutationChannelMessage): void {
    if (this.channel) {
      this.channel.postMessage(message);
    }
  }

  // ═══════════════════════════════════════════════════════════════════════
  // WINDOW MANAGEMENT
  // ═══════════════════════════════════════════════════════════════════════

  /**
   * Abre la ventana de mutation
   */
  openMutationWindow(): Window | null {
    // Si ya está abierta, enfocarla
    if (this.windowRef && !this.windowRef.closed) {
      this.windowRef.focus();
      return this.windowRef;
    }

    // Calcular tamaño y posición de la ventana
    const width = Math.min(1400, window.screen.availWidth * 0.8);
    const height = Math.min(900, window.screen.availHeight * 0.85);
    const left = (window.screen.availWidth - width) / 2;
    const top = (window.screen.availHeight - height) / 2;

    const features = [
      `width=${width}`,
      `height=${height}`,
      `left=${left}`,
      `top=${top}`,
      'menubar=no',
      'toolbar=no',
      'location=no',
      'status=no',
      'resizable=yes',
      'scrollbars=yes'
    ].join(',');

    this.windowRef = window.open('/mutation-window', 'ZAS-Mutation-Engine', features);
    this.windowReady.set(false);

    return this.windowRef;
  }

  /**
   * Cierra la ventana de mutation si está abierta
   */
  closeMutationWindow(): void {
    if (this.windowRef && !this.windowRef.closed) {
      this.windowRef.close();
    }
    this.windowRef = null;
    this.windowReady.set(false);
  }

  /**
   * Verifica si la ventana está abierta
   */
  isWindowOpen(): boolean {
    return this.windowRef !== null && !this.windowRef.closed;
  }

  /**
   * Resetea el estado
   */
  reset(): void {
    this.isStreaming.set(false);
    this.streamingPhase.set('idle');
    this.streamingProgress.set(0);
    this.streamingMessage.set('');
    this.streamingFiles.set(new Map());
    this.streamingPreviewHtml.set('');
    this.solicitud.set('');
    this.error.set(null);
    this.step.set('input');
    this.response.set(null);
  }
}
