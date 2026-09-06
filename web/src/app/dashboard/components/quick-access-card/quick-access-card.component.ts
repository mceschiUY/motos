import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatRippleModule } from '@angular/material/core';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-quick-access-card',
  standalone: true,
  imports: [CommonModule, MatIconModule, MatRippleModule, RouterModule],
  template: `
    <div class="quick-card"
         [class]="'color-' + color"
         [routerLink]="route"
         matRipple>
      <div class="card-header">
        <div class="card-icon">
          <mat-icon>{{ icon }}</mat-icon>
        </div>
        <mat-icon class="arrow-icon">arrow_forward</mat-icon>
      </div>
      <div class="card-content">
        <h3 class="card-title">{{ title }}</h3>
        <p class="card-description">{{ description }}</p>
      </div>
      @if (stats && stats.length > 0) {
        <div class="card-stats">
          @for (stat of stats; track stat.label) {
            <div class="mini-stat">
              <span class="mini-value">{{ stat.value }}</span>
              <span class="mini-label">{{ stat.label }}</span>
            </div>
          }
        </div>
      }
    </div>
  `,
  styles: [`
    .quick-card {
      display: flex;
      flex-direction: column;
      padding: 20px;
      background: var(--ceskia-surface);
      border: 1px solid var(--ceskia-border-default);
      border-radius: 16px;
      cursor: pointer;
      transition: all 0.2s ease;

      &:hover {
        transform: translateY(-4px);
        box-shadow: var(--ceskia-shadow-md);

        .arrow-icon {
          transform: translateX(4px);
          opacity: 1;
        }
      }

      &.color-cyan .card-icon { background: var(--ceskia-accent-primary-glow); mat-icon { color: var(--ceskia-accent-primary); } }
      &.color-green .card-icon { background: var(--ceskia-accent-success-glow); mat-icon { color: var(--ceskia-accent-success); } }
      &.color-purple .card-icon { background: var(--ceskia-accent-primary-glow); mat-icon { color: var(--ceskia-accent-primary-hover); } }
      &.color-blue .card-icon { background: var(--ceskia-accent-primary-glow); mat-icon { color: var(--ceskia-accent-primary); } }
      &.color-orange .card-icon { background: var(--ceskia-accent-warning-glow); mat-icon { color: var(--ceskia-accent-warning); } }
    }

    .card-header {
      display: flex;
      justify-content: space-between;
      align-items: flex-start;
      margin-bottom: 16px;
    }

    .card-icon {
      width: 48px;
      height: 48px;
      border-radius: 12px;
      display: flex;
      align-items: center;
      justify-content: center;

      mat-icon {
        font-size: 24px;
        width: 24px;
        height: 24px;
      }
    }

    .arrow-icon {
      color: var(--ceskia-text-tertiary);
      opacity: 0.5;
      transition: all 0.2s ease;
    }

    .card-content {
      flex: 1;
    }

    .card-title {
      font-size: 16px;
      font-weight: 600;
      color: var(--ceskia-text-primary);
      margin: 0 0 4px 0;
    }

    .card-description {
      font-size: 13px;
      color: var(--ceskia-text-secondary);
      margin: 0;
      line-height: 1.4;
    }

    .card-stats {
      display: flex;
      gap: 16px;
      margin-top: 16px;
      padding-top: 16px;
      border-top: 1px solid var(--ceskia-border-default);
    }

    .mini-stat {
      display: flex;
      flex-direction: column;
    }

    .mini-value {
      font-size: 18px;
      font-weight: 700;
      color: var(--ceskia-text-primary);
    }

    .mini-label {
      font-size: 11px;
      color: var(--ceskia-text-tertiary);
      text-transform: uppercase;
    }
  `]
})
export class QuickAccessCardComponent {
  @Input() title: string = '';
  @Input() description: string = '';
  @Input() icon: string = 'folder';
  @Input() route: string = '/';
  @Input() color: 'cyan' | 'green' | 'purple' | 'blue' | 'orange' = 'cyan';
  @Input() stats?: { label: string; value: number }[];
}
