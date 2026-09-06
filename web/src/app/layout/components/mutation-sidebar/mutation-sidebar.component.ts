import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatRippleModule } from '@angular/material/core';
import { MutationStateService } from '../../../core/services/mutation-state.service';
import { ESTADO_LABELS, MutacionEstado } from '../../../core/services/mutation.service';
import { SentinelService, CopyHealthResponse } from '../../../core/services/sentinel.service';
import { firstValueFrom } from 'rxjs';

// ═══════════════════════════════════════════════════════════════════════════════
// MUTATION SIDEBAR COMPONENT
// Modal para solicitar cambios en lenguaje natural (ZAS Mutation Engine)
// Flujo simplificado: input → executing → done
// ═══════════════════════════════════════════════════════════════════════════════

@Component({
  selector: 'app-mutation-sidebar',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatIconModule,
    MatButtonModule,
    MatProgressSpinnerModule,
    MatProgressBarModule,
    MatTooltipModule,
    MatRippleModule
  ],
  templateUrl: './mutation-sidebar.component.html',
  styleUrl: './mutation-sidebar.component.scss'
})
export class MutationSidebarComponent {
  readonly state = inject(MutationStateService);
  private readonly router = inject(Router);
  private readonly sentinelService = inject(SentinelService);

  solicitudText = '';
  estadoLabels = ESTADO_LABELS;

  // Copy health state
  readonly copyHealth = signal<CopyHealthResponse | null>(null);
  readonly isCheckingCopy = signal(false);
  readonly sentinelError = signal<string | null>(null);

  // Audio recording state
  isRecording = false;
  isProcessingAudio = false;
  private mediaRecorder: MediaRecorder | null = null;
  private audioChunks: Blob[] = [];

  onSolicitudChange(value: string): void {
    this.solicitudText = value;
    this.state.setSolicitud(value);
  }

  async onAnalyze(): Promise<void> {
    const rutaActual = this.router.url;
    const componenteActual = this.extractComponentName(rutaActual);

    const contexto = {
      rutaActual,
      componenteActual
    };

    await this.state.analyzeWithStreaming(contexto);
  }

  private extractComponentName(ruta: string): string {
    const parts = ruta.split('/').filter(p => p && !p.includes('?'));
    return parts.join('-') || 'dashboard';
  }

  onCancel(): void {
    this.solicitudText = '';
    this.state.cancel();
  }

  onNewMutation(): void {
    this.solicitudText = '';
    this.state.newMutation();
  }

  onClose(): void {
    this.solicitudText = '';
    this.state.close();
  }

  async onOpenFab(event: Event): Promise<void> {
    console.log('[ZAS] FAB clicked');
    event.preventDefault();
    event.stopPropagation();
    event.stopImmediatePropagation();
    this.state.open();
    console.log('[ZAS] Modal opened, isOpen:', this.state.isOpen());

    // Verificar estado de la copia al abrir
    await this.checkCopyHealth();
  }

  async checkCopyHealth(): Promise<void> {
    this.isCheckingCopy.set(true);
    this.sentinelError.set(null);

    try {
      const health = await firstValueFrom(this.sentinelService.getCopyHealth());
      this.copyHealth.set(health);
      console.log('[ZAS] Copy health:', health);
    } catch (err: any) {
      console.error('[ZAS] Error checking copy health:', err);
      this.sentinelError.set('No se pudo conectar con ZAS.Sentinel. Verificar que esté corriendo en puerto 5002.');
      this.copyHealth.set(null);
    } finally {
      this.isCheckingCopy.set(false);
    }
  }

  async onRetryCheckCopy(): Promise<void> {
    await this.checkCopyHealth();
  }

  get isCopyReady(): boolean {
    return this.copyHealth()?.status === 'ready';
  }

  async onRevert(): Promise<void> {
    await this.state.revertChanges();
  }

  getEstadoLabel(estado: MutacionEstado): string {
    return ESTADO_LABELS[estado] || 'Desconocido';
  }

  getEstadoClass(estado: MutacionEstado): string {
    switch (estado) {
      case MutacionEstado.Ejecutada: return 'estado-success';
      case MutacionEstado.Fallida: return 'estado-error';
      case MutacionEstado.Revertida: return 'estado-warning';
      case MutacionEstado.Analizada:
      case MutacionEstado.Previsualizada: return 'estado-info';
      default: return 'estado-pending';
    }
  }

  getPhaseLabel(phase: string): string {
    switch (phase) {
      case 'interpreting': return 'Interpretando solicitud';
      case 'analyzing': return 'Analizando arquitectura';
      case 'generating': return 'Generando codigo';
      case 'applying': return 'Aplicando cambios';
      case 'complete': return 'Completado';
      case 'error': return 'Error';
      default: return 'Procesando';
    }
  }

  getPhaseIcon(phase: string): string {
    switch (phase) {
      case 'interpreting': return 'psychology';
      case 'analyzing': return 'analytics';
      case 'generating': return 'code';
      case 'applying': return 'save';
      case 'complete': return 'done_all';
      case 'error': return 'error';
      default: return 'hourglass_empty';
    }
  }

  // ═══════════════════════════════════════════════════════════════════════
  // AUDIO RECORDING (preparado para Whisper)
  // ═══════════════════════════════════════════════════════════════════════

  async onAudioStart(event: Event): Promise<void> {
    event.preventDefault();

    if (this.isRecording || this.isProcessingAudio) return;

    try {
      const stream = await navigator.mediaDevices.getUserMedia({ audio: true });
      this.mediaRecorder = new MediaRecorder(stream);
      this.audioChunks = [];

      this.mediaRecorder.ondataavailable = (e) => {
        if (e.data.size > 0) {
          this.audioChunks.push(e.data);
        }
      };

      this.mediaRecorder.onstop = () => {
        stream.getTracks().forEach(track => track.stop());
        this.processAudio();
      };

      this.mediaRecorder.start();
      this.isRecording = true;
      console.log('[ZAS] Audio recording started');
    } catch (err) {
      console.error('[ZAS] Error accessing microphone:', err);
    }
  }

  onAudioStop(event: Event): void {
    event.preventDefault();

    if (!this.isRecording || !this.mediaRecorder) return;

    this.mediaRecorder.stop();
    this.isRecording = false;
    console.log('[ZAS] Audio recording stopped');
  }

  onAudioCancel(event: Event): void {
    if (!this.isRecording || !this.mediaRecorder) return;

    this.mediaRecorder.stop();
    this.isRecording = false;
    this.audioChunks = [];
    console.log('[ZAS] Audio recording cancelled');
  }

  private async processAudio(): Promise<void> {
    if (this.audioChunks.length === 0) return;

    this.isProcessingAudio = true;
    console.log('[ZAS] Processing audio...');

    const audioBlob = new Blob(this.audioChunks, { type: 'audio/webm' });
    console.log('[ZAS] Audio blob size:', audioBlob.size);

    // TODO: Integrar con Whisper API
    // Por ahora, simular transcripcion
    setTimeout(() => {
      // Placeholder - aqui se llamaria a Whisper
      const transcription = '[Audio grabado - Whisper pendiente de integrar]';
      this.solicitudText = this.solicitudText
        ? `${this.solicitudText}\n${transcription}`
        : transcription;
      this.state.setSolicitud(this.solicitudText);
      this.isProcessingAudio = false;
      console.log('[ZAS] Audio processed (placeholder)');
    }, 500);
  }
}
