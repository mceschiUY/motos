// ═══════════════════════════════════════════════════════════════════════════════
// MUTATION STREAM MODELS - Modelos para streaming en tiempo real
// Define las interfaces para los eventos SSE durante el análisis de mutaciones
// ═══════════════════════════════════════════════════════════════════════════════

/**
 * Tipos de eventos que puede enviar el stream
 */
export type StreamEventType =
  | 'start'           // Inicio del análisis
  | 'progress'        // Actualización de progreso
  | 'file_start'      // Inicio de generación de un archivo
  | 'file_chunk'      // Chunk de código de un archivo
  | 'file_complete'   // Archivo completado
  | 'preview_html'    // Preview HTML generado
  | 'analysis_complete' // Análisis completado
  | 'error';          // Error durante el proceso

/**
 * Evento base del stream
 */
export interface StreamEvent {
  type: StreamEventType;
  timestamp: number;
}

/**
 * Evento de inicio de análisis
 */
export interface StreamStartEvent extends StreamEvent {
  type: 'start';
  mutacionId: number;
  message: string;
}

/**
 * Evento de progreso
 */
export interface StreamProgressEvent extends StreamEvent {
  type: 'progress';
  phase: 'interpreting' | 'analyzing' | 'generating' | 'finalizing';
  percentage: number;
  message: string;
}

/**
 * Evento de inicio de generación de archivo
 */
export interface StreamFileStartEvent extends StreamEvent {
  type: 'file_start';
  fileId: string;
  fileName: string;
  filePath: string;
  language: 'typescript' | 'html' | 'scss' | 'csharp' | 'sql';
  layer: 'frontend' | 'backend' | 'database';
}

/**
 * Evento de chunk de código
 */
export interface StreamFileChunkEvent extends StreamEvent {
  type: 'file_chunk';
  fileId: string;
  content: string;
  isComplete: boolean;
}

/**
 * Evento de archivo completado
 */
export interface StreamFileCompleteEvent extends StreamEvent {
  type: 'file_complete';
  fileId: string;
  totalLines: number;
  action: 'create' | 'modify' | 'delete';
}

/**
 * Evento de preview HTML
 */
export interface StreamPreviewHtmlEvent extends StreamEvent {
  type: 'preview_html';
  html: string;
}

/**
 * Evento de análisis completado
 */
export interface StreamAnalysisCompleteEvent extends StreamEvent {
  type: 'analysis_complete';
  mutacionId: number;
  summary: string;
  architectureAudit: {
    isCompliant: boolean;
    compliancePercentage: number;
    warnings: string[];
    recommendations: string[];
    message?: string;
  };
  totalFiles: number;
}

/**
 * Evento de error
 */
export interface StreamErrorEvent extends StreamEvent {
  type: 'error';
  code: string;
  message: string;
}

/**
 * Unión de todos los tipos de eventos
 */
export type MutationStreamEvent =
  | StreamStartEvent
  | StreamProgressEvent
  | StreamFileStartEvent
  | StreamFileChunkEvent
  | StreamFileCompleteEvent
  | StreamPreviewHtmlEvent
  | StreamAnalysisCompleteEvent
  | StreamErrorEvent;

/**
 * Estado de un archivo durante la generación
 */
export interface GeneratedFileState {
  fileId: string;
  fileName: string;
  filePath: string;
  language: 'typescript' | 'html' | 'scss' | 'csharp' | 'sql';
  layer: 'frontend' | 'backend' | 'database';
  content: string;
  isComplete: boolean;
  action?: 'create' | 'modify' | 'delete';
  totalLines?: number;
}

/**
 * Estado completo del streaming
 */
export interface StreamingState {
  isStreaming: boolean;
  mutacionId: number | null;
  phase: 'idle' | 'interpreting' | 'analyzing' | 'generating' | 'finalizing' | 'complete' | 'error';
  progress: number;
  message: string;
  files: Map<string, GeneratedFileState>;
  previewHtml: string;
  error: string | null;
}

/**
 * Estado inicial del streaming
 */
export const INITIAL_STREAMING_STATE: StreamingState = {
  isStreaming: false,
  mutacionId: null,
  phase: 'idle',
  progress: 0,
  message: '',
  files: new Map(),
  previewHtml: '',
  error: null
};
