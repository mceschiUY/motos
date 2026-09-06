import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatBadgeModule } from '@angular/material/badge';
import { MatChipsModule } from '@angular/material/chips';
import { MatRippleModule } from '@angular/material/core';
import { EvolutionHubService } from '../../../core/services/evolution-hub.service';
import { Proposal } from '../../../core/services/sentinel.service';
import { EvolutionBoardService } from '../../../modules/evolution-board/services/evolution-board.service';
import { NotificationService } from '../../../core/services/notification.service';

@Component({
  selector: 'app-evolution-hub',
  standalone: true,
  imports: [
    CommonModule,
    MatIconModule,
    MatButtonModule,
    MatTooltipModule,
    MatProgressSpinnerModule,
    MatBadgeModule,
    MatChipsModule,
    MatRippleModule
  ],
  templateUrl: './evolution-hub.component.html',
  styleUrl: './evolution-hub.component.scss'
})
export class EvolutionHubComponent {
  readonly hub = inject(EvolutionHubService);
  private readonly boardService = inject(EvolutionBoardService);
  private readonly notification = inject(NotificationService);

  onMaterialize(proposal: Proposal): void {
    this.hub.materializeProposal(proposal);
  }

  onAddToBoard(proposal: Proposal): void {
    this.boardService.importProposals([proposal]).subscribe({
      next: (result) => {
        if (result.imported > 0) {
          this.notification.success(`"${proposal.featureName}" agregado al Board`);
        } else {
          this.notification.info(`"${proposal.featureName}" ya existe en el Board`);
        }
      },
      error: () => this.notification.error('Error al agregar al Board')
    });
  }

  onRefresh(): void {
    this.hub.refreshProposals();
  }

  getPriorityClass(priority: string): string {
    return `priority-${priority}`;
  }

  getTabCount(index: number): number {
    switch (index) {
      case 0: return this.hub.filteredOptimization().length;
      case 1: return this.hub.filteredExpansion().length;
      case 2: return this.hub.filteredCrossCutting().length;
      default: return 0;
    }
  }
}
