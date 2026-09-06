import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatRippleModule } from '@angular/material/core';
import { EvolutionItem } from './services/evolution-board.service';

@Component({
  selector: 'app-evolution-board-card',
  standalone: true,
  imports: [CommonModule, MatIconModule, MatTooltipModule, MatRippleModule],
  template: `
    <div class="board-card" [class]="'priority-' + item.prioridad" matRipple>
      <div class="card-top">
        <mat-icon class="card-icon">{{ item.icon }}</mat-icon>
        <span class="card-title">{{ item.titulo }}</span>
        <span class="card-priority">{{ item.prioridad }}</span>
      </div>
      @if (item.entidadRelacionada) {
        <span class="card-entity">{{ item.entidadRelacionada }}</span>
      }
      <p class="card-desc">{{ item.descripcion | slice:0:80 }}{{ item.descripcion.length > 80 ? '...' : '' }}</p>
      @if (item.errorMessage) {
        <div class="card-error">
          <mat-icon>error</mat-icon>
          <span>{{ item.errorMessage | slice:0:60 }}</span>
        </div>
      }
      <div class="card-footer">
        <span class="card-category">{{ item.categoria }}</span>
        @if (item.estado === 'Review') {
          <div class="card-action-btns">
            <button class="action-btn commit" (click)="onCommit.emit(item)" matTooltip="Desplegar">
              <mat-icon>check</mat-icon>
            </button>
            <button class="action-btn reject" (click)="onReject.emit(item)" matTooltip="Rechazar">
              <mat-icon>close</mat-icon>
            </button>
          </div>
        }
        @if (item.estado === 'Failed' || item.estado === 'Rejected') {
          <button class="action-btn retry" (click)="onRetry.emit(item)" matTooltip="Volver a Backlog">
            <mat-icon>replay</mat-icon>
          </button>
        }
      </div>
    </div>
  `,
  styles: [`
    :host { display: block; }

    .board-card {
      padding: 12px;
      border-radius: 8px;
      background: var(--ceskia-surface);
      border: 1px solid var(--ceskia-border-default);
      cursor: grab;
      transition: all 0.2s ease;

      &:hover {
        transform: translateY(-1px);
        box-shadow: var(--ceskia-shadow-md);
      }

      &:active { cursor: grabbing; }

      &.priority-high { border-left: 3px solid var(--ceskia-accent-warning); }
      &.priority-medium { border-left: 3px solid var(--ceskia-accent-primary); }
      &.priority-low { border-left: 3px solid var(--ceskia-text-tertiary); }
    }

    .card-top {
      display: flex;
      align-items: center;
      gap: 8px;
      margin-bottom: 4px;
    }

    .card-icon {
      font-size: 16px;
      width: 16px;
      height: 16px;
      color: var(--ceskia-accent-info);
    }

    .card-title {
      flex: 1;
      font-size: 13px;
      font-weight: 600;
      color: var(--ceskia-text-primary);
      white-space: nowrap;
      overflow: hidden;
      text-overflow: ellipsis;
    }

    .card-priority {
      font-size: 9px;
      text-transform: uppercase;
      letter-spacing: 0.5px;
      padding: 2px 6px;
      border-radius: 4px;
      background: var(--ceskia-hover);
      color: var(--ceskia-text-secondary);
    }

    .card-entity {
      font-size: 11px;
      color: var(--ceskia-accent-primary);
      text-transform: capitalize;
    }

    .card-desc {
      font-size: 11px;
      color: var(--ceskia-text-secondary);
      line-height: 1.4;
      margin: 4px 0;
    }

    .card-error {
      display: flex;
      align-items: center;
      gap: 4px;
      padding: 4px 8px;
      border-radius: 4px;
      background: var(--ceskia-accent-danger-glow);
      margin: 4px 0;

      mat-icon {
        font-size: 14px;
        width: 14px;
        height: 14px;
        color: var(--ceskia-accent-danger);
      }
      span {
        font-size: 10px;
        color: var(--ceskia-accent-danger);
      }
    }

    .card-footer {
      display: flex;
      align-items: center;
      justify-content: space-between;
      margin-top: 6px;
    }

    .card-category {
      font-size: 10px;
      color: var(--ceskia-text-tertiary);
      text-transform: capitalize;
    }

    .card-action-btns {
      display: flex;
      gap: 4px;
    }

    .action-btn {
      width: 24px;
      height: 24px;
      border-radius: 6px;
      border: none;
      cursor: pointer;
      display: flex;
      align-items: center;
      justify-content: center;
      transition: all 0.2s ease;

      mat-icon {
        font-size: 14px;
        width: 14px;
        height: 14px;
      }

      &.commit {
        background: var(--ceskia-accent-success-glow);
        color: var(--ceskia-accent-success);
        &:hover { background: rgba(30, 190, 87, 0.22); }
      }

      &.reject {
        background: var(--ceskia-accent-danger-glow);
        color: var(--ceskia-accent-danger);
        &:hover { background: rgba(239, 68, 68, 0.22); }
      }

      &.retry {
        background: var(--ceskia-accent-primary-glow);
        color: var(--ceskia-accent-primary);
        &:hover { background: rgba(0, 159, 227, 0.2); }
      }
    }
  `]
})
export class EvolutionBoardCardComponent {
  @Input({ required: true }) item!: EvolutionItem;
  @Output() onCommit = new EventEmitter<EvolutionItem>();
  @Output() onReject = new EventEmitter<EvolutionItem>();
  @Output() onRetry = new EventEmitter<EvolutionItem>();
}
