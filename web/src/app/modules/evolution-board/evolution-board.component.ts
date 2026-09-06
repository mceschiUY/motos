import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { CdkDragDrop, DragDropModule, moveItemInArray, transferArrayItem } from '@angular/cdk/drag-drop';
import { EvolutionBoardService, EvolutionItem, EvolutionItemEstado, BoardColumn } from './services/evolution-board.service';
import { EvolutionBoardCardComponent } from './evolution-board-card.component';
import { MutationStateService } from '../../core/services/mutation-state.service';
import { EvolutionHubService } from '../../core/services/evolution-hub.service';
import { SentinelService } from '../../core/services/sentinel.service';
import { NotificationService } from '../../core/services/notification.service';

@Component({
  selector: 'app-evolution-board',
  standalone: true,
  imports: [
    CommonModule,
    MatIconModule,
    MatButtonModule,
    MatTooltipModule,
    MatProgressSpinnerModule,
    DragDropModule,
    EvolutionBoardCardComponent
  ],
  templateUrl: './evolution-board.component.html',
  styleUrl: './evolution-board.component.scss'
})
export class EvolutionBoardComponent implements OnInit {
  readonly boardService = inject(EvolutionBoardService);
  private readonly mutationState = inject(MutationStateService);
  private readonly hubService = inject(EvolutionHubService);
  private readonly sentinelService = inject(SentinelService);
  private readonly notification = inject(NotificationService);

  ngOnInit(): void {
    this.boardService.loadBoard();
  }

  getConnectedLists(): string[] {
    return this.boardService.columns().map(c => 'column-' + c.id);
  }

  onDrop(event: CdkDragDrop<EvolutionItem[]>, targetColumn: BoardColumn): void {
    if (event.previousContainer === event.container) {
      // Reorder within column
      moveItemInArray(event.container.data, event.previousIndex, event.currentIndex);
      const item = event.container.data[event.currentIndex];
      this.boardService.reorderItem(item.id, event.currentIndex).subscribe();
    } else {
      // Move between columns
      transferArrayItem(
        event.previousContainer.data,
        event.container.data,
        event.previousIndex,
        event.currentIndex
      );
      const item = event.container.data[event.currentIndex];
      this.boardService.moveItem(item.id, targetColumn.id).subscribe({
        next: () => {
          // Auto-trigger build when moved to Building
          if (targetColumn.id === 'Building') {
            this.triggerBuild(item);
          }
        },
        error: (err) => {
          this.notification.error('Error al mover item');
          this.boardService.loadBoard(); // Reload on error
        }
      });
    }
  }

  onImportProposals(): void {
    this.sentinelService.getProposals().subscribe({
      next: (proposals) => {
        const allProposals = [
          ...proposals.optimization,
          ...proposals.expansion,
          ...proposals.crossCutting
        ];
        this.boardService.importProposals(allProposals).subscribe({
          next: (result) => {
            this.notification.success(`${result.imported} propuestas importadas, ${result.skipped} ya existentes`);
            this.boardService.loadBoard();
          },
          error: () => this.notification.error('Error al importar propuestas')
        });
      },
      error: () => this.notification.error('Error al obtener propuestas del Hub')
    });
  }

  onCommitItem(item: EvolutionItem): void {
    this.boardService.moveItem(item.id, 'Deployed').subscribe({
      next: () => {
        this.notification.success(`"${item.titulo}" desplegado`);
        this.boardService.loadBoard();
      }
    });
  }

  onRejectItem(item: EvolutionItem): void {
    this.boardService.moveItem(item.id, 'Rejected').subscribe({
      next: () => {
        this.notification.info(`"${item.titulo}" rechazado`);
        this.boardService.loadBoard();
      }
    });
  }

  onRetryItem(item: EvolutionItem): void {
    this.boardService.moveItem(item.id, 'Backlog').subscribe({
      next: () => {
        this.notification.info(`"${item.titulo}" movido a Backlog`);
        this.boardService.loadBoard();
      }
    });
  }

  onDeleteItem(item: EvolutionItem): void {
    this.boardService.deleteItem(item.id).subscribe({
      next: () => {
        this.notification.info(`"${item.titulo}" archivado`);
        this.boardService.loadBoard();
      }
    });
  }

  private triggerBuild(item: EvolutionItem): void {
    this.mutationState.setSolicitud(item.promptMutation);
    this.mutationState.open();
    this.notification.info(`Construyendo: "${item.titulo}" - Mutation sidebar abierto`);
  }
}
