import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatSortModule } from '@angular/material/sort';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatChipsModule } from '@angular/material/chips';
import { MatMenuModule } from '@angular/material/menu';
import { BreadcrumbComponent, BreadcrumbItem } from '../../../../../shared/components/breadcrumb/breadcrumb.component';
import { SeguridadService } from '../../../services/seguridad.service';
import { Rol } from '../../../models/seguridad.model';
import { RolFormComponent } from '../rol-form/rol-form.component';
import { RolCapabilitiesComponent } from '../rol-capabilities/rol-capabilities.component';
import { confirmar } from '../../../../../shared/confirm/confirmar';

@Component({
  selector: 'app-rol-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatTableModule,
    MatSortModule,
    MatButtonModule,
    MatIconModule,
    MatDialogModule,
    MatSnackBarModule,
    MatTooltipModule,
    MatChipsModule,
    MatMenuModule,
    BreadcrumbComponent
  ],
  templateUrl: './rol-list.component.html',
  styleUrl: './rol-list.component.scss'
})
export class RolListComponent implements OnInit {
  private readonly service = inject(SeguridadService);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);

  // Signals
  readonly isLoading = signal<boolean>(false);
  readonly items = signal<Rol[]>([]);
  readonly searchTerm = signal<string>('');

  // Computed
  readonly filteredItems = computed(() => {
    const items = this.items();
    const search = this.searchTerm().toLowerCase().trim();
    if (!search) return items;
    return items.filter(item =>
      item.nombre.toLowerCase().includes(search) ||
      item.descripcion.toLowerCase().includes(search)
    );
  });

  readonly totalCount = computed(() => this.items().length);
  readonly activeCount = computed(() => this.items().filter(i => i.activo).length);

  // Table
  displayedColumns = ['nombre', 'descripcion', 'cantidadCapabilities', 'activo', 'actions'];
  dataSource = new MatTableDataSource<Rol>([]);
  readonly skeletonRows = Array(5).fill(0);

  breadcrumbItems: BreadcrumbItem[] = [
    { label: 'Inicio', icon: 'home' },
    { label: 'Seguridad', icon: 'security' },
    { label: 'Roles', icon: 'badge' }
  ];

  ngOnInit(): void {
    this.loadData();
  }

  loadData(): void {
    this.isLoading.set(true);

    this.service.getRoles().subscribe({
      next: (data) => {
        this.items.set(data);
        this.dataSource.data = data;
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('[RolList] Error cargando datos:', err);
        this.isLoading.set(false);
        this.showMessage('Error al cargar roles', 'error');
      }
    });
  }

  onSearchChange(event: Event): void {
    const value = (event.target as HTMLInputElement).value;
    this.searchTerm.set(value);
    this.dataSource.data = this.filteredItems();
  }

  openCreate(): void {
    const dialogRef = this.dialog.open(RolFormComponent, {
      width: '500px',
      data: { rol: null }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.loadData();
        this.showMessage('Rol creado correctamente', 'success');
      }
    });
  }

  openEdit(rol: Rol): void {
    const dialogRef = this.dialog.open(RolFormComponent, {
      width: '500px',
      data: { rol }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.loadData();
        this.showMessage('Rol actualizado correctamente', 'success');
      }
    });
  }

  openCapabilities(rol: Rol): void {
    const dialogRef = this.dialog.open(RolCapabilitiesComponent, {
      width: '700px',
      maxHeight: '80vh',
      data: { rol }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.loadData();
        this.showMessage('Capabilities actualizadas', 'success');
      }
    });
  }

  toggleActivo(rol: Rol): void {
    this.service.toggleRolActivo(rol.id).subscribe({
      next: () => {
        this.loadData();
        this.showMessage(`Rol ${rol.activo ? 'desactivado' : 'activado'}`, 'success');
      },
      error: (err) => {
        console.error('[RolList] Error toggle activo:', err);
        this.showMessage('Error al cambiar estado', 'error');
      }
    });
  }

  async deleteItem(rol: Rol): Promise<void> {
    if (await confirmar(`¿Desactivar el rol "${rol.nombre}"?\n\nEl rol no será eliminado, solo desactivado.`)) {
      this.service.eliminarRol(rol.id).subscribe({
        next: () => {
          this.loadData();
          this.showMessage('Rol desactivado', 'success');
        },
        error: (err) => {
          console.error('[RolList] Error eliminando:', err);
          this.showMessage('Error al desactivar rol', 'error');
        }
      });
    }
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
