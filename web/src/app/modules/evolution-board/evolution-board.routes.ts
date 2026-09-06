import { Routes } from '@angular/router';

export const EVOLUTION_BOARD_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./evolution-board.component').then(m => m.EvolutionBoardComponent)
  }
];
