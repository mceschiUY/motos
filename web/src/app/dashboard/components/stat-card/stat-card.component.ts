import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatRippleModule } from '@angular/material/core';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-stat-card',
  standalone: true,
  imports: [CommonModule, MatIconModule, MatRippleModule, RouterModule],
  template: `
    <div class="stat-card"
         [class]="'color-' + color"
         [class.clickable]="route"
         [routerLink]="route"
         matRipple
         [matRippleDisabled]="!route">
      <div class="stat-icon">
        <mat-icon>{{ icon }}</mat-icon>
      </div>
      <div class="stat-content">
        <span class="stat-value">{{ value | number }}</span>
        <span class="stat-label">{{ label }}</span>
        @if (subValue !== undefined && subValue !== null) {
          <span class="stat-subvalue" [class]="subValueColor">
            {{ subValuePrefix }}{{ subValue | number }}{{ subValueSuffix }}
          </span>
        }
      </div>
    </div>
  `,
  styles: [`
    .stat-card {
      display: flex;
      align-items: center;
      gap: 16px;
      padding: 20px;
      background: var(--ceskia-surface);
      border: 1px solid var(--ceskia-border-default);
      border-radius: 16px;
      transition: all 0.2s ease;

      &.clickable {
        cursor: pointer;
        &:hover {
          transform: translateY(-2px);
          box-shadow: var(--ceskia-shadow-md);
        }
      }

      &.color-cyan .stat-icon { background: var(--ceskia-accent-primary-glow); mat-icon { color: var(--ceskia-accent-primary); } }
      &.color-green .stat-icon { background: var(--ceskia-accent-success-glow); mat-icon { color: var(--ceskia-accent-success); } }
      &.color-purple .stat-icon { background: var(--ceskia-accent-primary-glow); mat-icon { color: var(--ceskia-accent-primary-hover); } }
      &.color-blue .stat-icon { background: var(--ceskia-accent-primary-glow); mat-icon { color: var(--ceskia-accent-primary); } }
      &.color-orange .stat-icon { background: var(--ceskia-accent-warning-glow); mat-icon { color: var(--ceskia-accent-warning); } }
      &.color-red .stat-icon { background: var(--ceskia-accent-danger-glow); mat-icon { color: var(--ceskia-accent-danger); } }
    }

    .stat-icon {
      width: 56px;
      height: 56px;
      border-radius: 14px;
      display: flex;
      align-items: center;
      justify-content: center;
      flex-shrink: 0;

      mat-icon {
        font-size: 28px;
        width: 28px;
        height: 28px;
      }
    }

    .stat-content {
      display: flex;
      flex-direction: column;
      flex: 1;
      min-width: 0;
    }

    .stat-value {
      font-size: 28px;
      font-weight: 700;
      color: var(--ceskia-text-primary);
      line-height: 1.2;
    }

    .stat-label {
      font-size: 13px;
      color: var(--ceskia-text-tertiary);
      text-transform: uppercase;
      letter-spacing: 0.5px;
    }

    .stat-subvalue {
      font-size: 12px;
      margin-top: 4px;
      &.success { color: var(--ceskia-accent-success); }
      &.warning { color: var(--ceskia-accent-warning); }
      &.error { color: var(--ceskia-accent-danger); }
    }
  `]
})
export class StatCardComponent {
  @Input() icon: string = 'analytics';
  @Input() value: number = 0;
  @Input() label: string = '';
  @Input() color: 'cyan' | 'green' | 'purple' | 'blue' | 'orange' | 'red' = 'cyan';
  @Input() route?: string;
  @Input() subValue?: number;
  @Input() subValuePrefix: string = '';
  @Input() subValueSuffix: string = '';
  @Input() subValueColor: 'success' | 'warning' | 'error' = 'success';
}
