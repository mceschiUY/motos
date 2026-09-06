import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatSortModule } from '@angular/material/sort';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatChipsModule } from '@angular/material/chips';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { BreadcrumbComponent, BreadcrumbItem } from '../../../../../shared/components/breadcrumb/breadcrumb.component';
import { SeguridadService } from '../../../services/seguridad.service';
import { Capability } from '../../../models/seguridad.model';

@Component({
  selector: 'app-capability-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatTableModule,
    MatSortModule,
    MatButtonModule,
    MatIconModule,
    MatSnackBarModule,
    MatTooltipModule,
    MatChipsModule,
    MatSelectModule,
    MatFormFieldModule,
    BreadcrumbComponent
  ],
  templateUrl: './capability-list.component.html',
  styleUrl: './capability-list.component.scss'
})
export class CapabilityListComponent implements OnInit {
  private readonly service = inject(SeguridadService);
  private readonly snackBar = inject(MatSnackBar);

  // Signals
  readonly isLoading = signal<boolean>(false);
  readonly items = signal<Capability[]>([]);
  readonly modulos = signal<string[]>([]);
  readonly searchTerm = signal<string>('');
  readonly selectedModulo = signal<string>('');

  // Computed
  readonly filteredItems = computed(() => {
    let items = this.items();
    const search = this.searchTerm().toLowerCase().trim();
    const modulo = this.selectedModulo();

    if (modulo) {
      items = items.filter(item => item.modulo === modulo);
    }

    if (search) {
      items = items.filter(item =>
        item.nombre.toLowerCase().includes(search) ||
        item.descripcion.toLowerCase().includes(search) ||
        item.modulo.toLowerCase().includes(search)
      );
    }

    return items;
  });

  readonly totalCount = computed(() => this.items().length);
  readonly filteredCount = computed(() => this.filteredItems().length);
  readonly modulosCount = computed(() => this.modulos().length);

  // Table
  displayedColumns = ['nombre', 'descripcion', 'modulo', 'activo'];
  dataSource = new MatTableDataSource<Capability>([]);
  readonly skeletonRows = Array(8).fill(0);

  breadcrumbItems: BreadcrumbItem[] = [
    { label: 'Inicio', icon: 'home' },
    { label: 'Seguridad', icon: 'security' },
    { label: 'Capabilities', icon: 'key' }
  ];

  ngOnInit(): void {
    this.loadData();
    this.loadModulos();
  }

  loadData(): void {
    this.isLoading.set(true);

    this.service.getCapabilities().subscribe({
      next: (data) => {
        this.items.set(data);
        this.dataSource.data = data;
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('[CapabilityList] Error cargando datos:', err);
        this.isLoading.set(false);
        this.showMessage('Error al cargar capabilities', 'error');
      }
    });
  }

  loadModulos(): void {
    this.service.getModulos().subscribe({
      next: (data) => {
        this.modulos.set(data);
      },
      error: (err) => {
        console.error('[CapabilityList] Error cargando modulos:', err);
      }
    });
  }

  onSearchChange(event: Event): void {
    const value = (event.target as HTMLInputElement).value;
    this.searchTerm.set(value);
    this.updateDataSource();
  }

  onModuloChange(modulo: string): void {
    this.selectedModulo.set(modulo);
    this.updateDataSource();
  }

  clearFilters(): void {
    this.searchTerm.set('');
    this.selectedModulo.set('');
    this.updateDataSource();
  }

  updateDataSource(): void {
    this.dataSource.data = this.filteredItems();
  }

  private showMessage(message: string, type: 'success' | 'error'): void {
    this.snackBar.open(message, 'Cerrar', {
      duration: 4000,
      horizontalPosition: 'end',
      verticalPosition: 'top',
      panelClass: type === 'error' ? 'snackbar-error' : 'snackbar-success'
    });
  }
}
