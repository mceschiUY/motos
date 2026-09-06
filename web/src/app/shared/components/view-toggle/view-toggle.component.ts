import { Component, EventEmitter, Input, Output, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';

export type ViewMode = 'table' | 'cards';

@Component({
  selector: 'app-view-toggle',
  standalone: true,
  imports: [CommonModule, MatButtonToggleModule, MatIconModule, MatTooltipModule],
  template: `
    <mat-button-toggle-group
      [value]="viewMode"
      (change)="onViewChange($event.value)"
      class="view-toggle">
      <mat-button-toggle value="table" matTooltip="Vista de tabla">
        <mat-icon>table_rows</mat-icon>
      </mat-button-toggle>
      <mat-button-toggle value="cards" matTooltip="Vista de tarjetas">
        <mat-icon>grid_view</mat-icon>
      </mat-button-toggle>
    </mat-button-toggle-group>
  `,
  styles: [`
    :host {
      display: inline-flex;
    }

    .view-toggle {
      border: 1px solid var(--ceskia-border-default) !important;
      border-radius: 8px !important;
      overflow: hidden;
      background: var(--ceskia-surface) !important;
    }

    :host ::ng-deep {
      .mat-button-toggle-group {
        border: none !important;
        background: var(--ceskia-surface) !important;
      }

      .mat-button-toggle {
        background: var(--ceskia-surface) !important;
        color: var(--ceskia-text-secondary) !important;
        border: none !important;

        &:hover {
          background: var(--ceskia-hover) !important;
        }

        &.mat-button-toggle-checked {
          background: var(--ceskia-accent-primary) !important;
          color: #ffffff !important;
        }

        .mat-button-toggle-button {
          height: 36px !important;
          width: 40px !important;
          padding: 0 !important;
        }

        .mat-button-toggle-label-content {
          padding: 0 !important;
          display: flex !important;
          align-items: center !important;
          justify-content: center !important;
        }

        mat-icon {
          font-size: 20px !important;
          width: 20px !important;
          height: 20px !important;
          line-height: 20px !important;
        }

        // Ocultar el check que aparece cuando está seleccionado
        .mat-pseudo-checkbox,
        .mdc-switch__icons,
        .mat-mdc-button-toggle-checked .mat-icon + .mat-icon {
          display: none !important;
        }
      }

      .mat-button-toggle-appearance-standard {
        background: var(--ceskia-surface) !important;
        color: var(--ceskia-text-secondary) !important;

        &.mat-button-toggle-checked {
          background: var(--ceskia-accent-primary) !important;
          color: #ffffff !important;
        }
      }

      // Quitar el separador entre botones
      .mat-button-toggle + .mat-button-toggle {
        border-left: 1px solid var(--ceskia-border-default) !important;
      }

      // Focus state
      .mat-button-toggle-focus-overlay {
        background: transparent !important;
      }

      // Ripple
      .mat-ripple {
        display: none !important;
      }

      // Ocultar cualquier icono de check adicional
      .mat-pseudo-checkbox {
        display: none !important;
      }
    }
  `]
})
export class ViewToggleComponent implements OnInit {
  @Input() storageKey = 'defaultViewMode';
  @Input() viewMode: ViewMode = 'table';
  @Output() viewModeChange = new EventEmitter<ViewMode>();

  ngOnInit(): void {
    const saved = localStorage.getItem(this.storageKey) as ViewMode;
    if (saved && (saved === 'table' || saved === 'cards')) {
      this.viewMode = saved;
      this.viewModeChange.emit(this.viewMode);
    }
  }

  onViewChange(mode: ViewMode): void {
    this.viewMode = mode;
    localStorage.setItem(this.storageKey, mode);
    this.viewModeChange.emit(mode);
  }
}
