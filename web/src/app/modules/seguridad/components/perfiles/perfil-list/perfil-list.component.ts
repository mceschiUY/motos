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
import { Perfil } from '../../../models/seguridad.model';
import { PerfilFormComponent } from '../perfil-form/perfil-form.component';
import { PerfilRolesComponent } from '../perfil-roles/perfil-roles.component';
import { confirmar } from '../../../../../shared/confirm/confirmar';

@Component({
  selector: 'app-perfil-list',
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
  templateUrl: './perfil-list.component.html',
  styleUrl: './perfil-list.component.scss'
})
export class PerfilListComponent implements OnInit {
  private readonly service = inject(SeguridadService);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);

  // Signals
  readonly isLoading = signal<boolean>(false);
  readonly items = signal<Perfil[]>([]);
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
  displayedColumns = ['nombre', 'descripcion', 'cantidadRoles', 'cantidadUsuarios', 'activo', 'actions'];
  dataSource = new MatTableDataSource<Perfil>([]);
  readonly skeletonRows = Array(5).fill(0);

  breadcrumbItems: BreadcrumbItem[] = [
    { label: 'Inicio', icon: 'home' },
    { label: 'Seguridad', icon: 'security' },
    { label: 'Perfiles', icon: 'folder_shared' }
  ];

  ngOnInit(): void {
    this.loadData();
  }

  loadData(): void {
    this.isLoading.set(true);

    this.service.getPerfiles().subscribe({
      next: (data) => {
        this.items.set(data);
        this.dataSource.data = data;
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('[PerfilList] Error cargando datos:', err);
        this.isLoading.set(false);
        this.showMessage('Error al cargar perfiles', 'error');
      }
    });
  }

  onSearchChange(event: Event): void {
    const value = (event.target as HTMLInputElement).value;
    this.searchTerm.set(value);
    this.dataSource.data = this.filteredItems();
  }

  openCreate(): void {
    const dialogRef = this.dialog.open(PerfilFormComponent, {
      width: '500px',
      data: { perfil: null }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.loadData();
        this.showMessage('Perfil creado correctamente', 'success');
      }
    });
  }

  openEdit(perfil: Perfil): void {
    const dialogRef = this.dialog.open(PerfilFormComponent, {
      width: '500px',
      data: { perfil }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.loadData();
        this.showMessage('Perfil actualizado correctamente', 'success');
      }
    });
  }

  openRoles(perfil: Perfil): void {
    const dialogRef = this.dialog.open(PerfilRolesComponent, {
      width: '600px',
      maxHeight: '80vh',
      data: { perfil }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.loadData();
        this.showMessage('Roles actualizados', 'success');
      }
    });
  }

  toggleActivo(perfil: Perfil): void {
    this.service.togglePerfilActivo(perfil.id).subscribe({
      next: () => {
        this.loadData();
        this.showMessage(`Perfil ${perfil.activo ? 'desactivado' : 'activado'}`, 'success');
      },
      error: (err) => {
        console.error('[PerfilList] Error toggle activo:', err);
        this.showMessage('Error al cambiar estado', 'error');
      }
    });
  }

  async deleteItem(perfil: Perfil): Promise<void> {
    if (await confirmar(`¿Desactivar el perfil "${perfil.nombre}"?\n\nEl perfil no será eliminado, solo desactivado.`)) {
      this.service.eliminarPerfil(perfil.id).subscribe({
        next: () => {
          this.loadData();
          this.showMessage('Perfil desactivado', 'success');
        },
        error: (err) => {
          console.error('[PerfilList] Error eliminando:', err);
          this.showMessage('Error al desactivar perfil', 'error');
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
