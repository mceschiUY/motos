import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatSortModule } from '@angular/material/sort';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-auditoria-list',
  standalone: true,
  imports: [
    CommonModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    MatInputModule,
    MatFormFieldModule,
    MatButtonModule,
    MatIconModule,
    MatCardModule,
    FormsModule
  ],
  templateUrl: './auditoria-list.component.html',
  styleUrls: ['./auditoria-list.component.scss']
})
export class AuditoriaListComponent implements OnInit {
  title = 'Auditoría';
  searchTerm = '';
  
  displayedColumns: string[] = ['fecha', 'usuario', 'accion', 'entidad', 'detalle'];
  dataSource: any[] = [];

  ngOnInit(): void {
    this.loadData();
  }

  loadData(): void {
    // Cargar datos de auditoría
  }

  search(): void {
    // Implementar búsqueda
  }

  clearSearch(): void {
    this.searchTerm = '';
    this.loadData();
  }
}