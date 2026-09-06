import {
  Component,
  OnInit,
  OnDestroy,
  inject,
  computed,
  ChangeDetectionStrategy
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MutationChannelService } from '../../../core/services/mutation-channel.service';

// ═══════════════════════════════════════════════════════════════════════════════
// MUTATION WINDOW COMPONENT
// Componente standalone para la ventana de mutation
// Se comunica con la ventana principal via BroadcastChannel
// ═══════════════════════════════════════════════════════════════════════════════

@Component({
  selector: 'app-mutation-window',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatIconModule,
    MatButtonModule,
    MatProgressSpinnerModule,
    MatProgressBarModule,
    MatTooltipModule
  ],
  templateUrl: './mutation-window.component.html',
  styleUrl: './mutation-window.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class MutationWindowComponent implements OnInit, OnDestroy {
  readonly channel = inject(MutationChannelService);

  readonly step = computed(() => this.channel.step());
  readonly isStreaming = computed(() => this.channel.isStreaming());
  readonly streamingPhase = computed(() => this.channel.streamingPhase());
  readonly streamingProgress = computed(() => this.channel.streamingProgress());
  readonly streamingMessage = computed(() => this.channel.streamingMessage());
  readonly error = computed(() => this.channel.error());
  readonly response = computed(() => this.channel.response());
  readonly solicitud = computed(() => this.channel.solicitud());

  ngOnInit(): void {
    this.channel.notifyWindowReady();
    document.title = 'ZAS Mutation Engine';
  }

  ngOnDestroy(): void {}

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

  closeWindow(): void {
    window.close();
  }
}
