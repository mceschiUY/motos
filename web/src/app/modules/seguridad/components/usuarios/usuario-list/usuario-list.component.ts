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
import { Usuario } from '../../../models/seguridad.model';
import { UsuarioFormComponent } from '../usuario-form/usuario-form.component';
import { UsuarioPasswordComponent } from '../usuario-password/usuario-password.component';
import { confirmar } from '../../../../../shared/confirm/confirmar';

@Component({
  selector: 'app-usuario-list',
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
  templateUrl: './usuario-list.component.html',
  styleUrl: './usuario-list.component.scss'
})
export class UsuarioListComponent implements OnInit {
  private readonly service = inject(SeguridadService);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);

  // Signals
  readonly isLoading = signal<boolean>(false);
  readonly items = signal<Usuario[]>([]);
  readonly searchTerm = signal<string>('');

  // Computed
  readonly filteredItems = computed(() => {
    const items = this.items();
    const search = this.searchTerm().toLowerCase().trim();
    if (!search) return items;
    return items.filter(item =>
      item.nombreCompleto.toLowerCase().includes(search) ||
      item.email.toLowerCase().includes(search) ||
      item.userName.toLowerCase().includes(search)
    );
  });

  readonly totalCount = computed(() => this.items().length);
  readonly activeCount = computed(() => this.items().filter(i => i.activo).length);

  // Table
  displayedColumns = ['nombre', 'email', 'perfil', 'activo', 'actions'];
  dataSource = new MatTableDataSource<Usuario>([]);
  readonly skeletonRows = Array(5).fill(0);

  breadcrumbItems: BreadcrumbItem[] = [
    { label: 'Inicio', icon: 'home' },
    { label: 'Seguridad', icon: 'security' },
    { label: 'Usuarios', icon: 'people' }
  ];

  ngOnInit(): void {
    this.loadData();
  }

  loadData(): void {
    this.isLoading.set(true);

    this.service.getUsuarios().subscribe({
      next: (data) => {
        this.items.set(data);
        this.dataSource.data = data;
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('[UsuarioList] Error cargando datos:', err);
        this.isLoading.set(false);
        this.showMessage('Error al cargar usuarios', 'error');
      }
    });
  }

  onSearchChange(event: Event): void {
    const value = (event.target as HTMLInputElement).value;
    this.searchTerm.set(value);
    this.dataSource.data = this.filteredItems();
  }

  openCreate(): void {
    const dialogRef = this.dialog.open(UsuarioFormComponent, {
      width: '550px',
      data: { usuario: null }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.loadData();
        this.showMessage('Usuario creado correctamente', 'success');
      }
    });
  }

  openEdit(usuario: Usuario): void {
    const dialogRef = this.dialog.open(UsuarioFormComponent, {
      width: '550px',
      data: { usuario }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.loadData();
        this.showMessage('Usuario actualizado correctamente', 'success');
      }
    });
  }

  openChangePassword(usuario: Usuario): void {
    const dialogRef = this.dialog.open(UsuarioPasswordComponent, {
      width: '450px',
      data: { usuario }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.showMessage('Contrasena actualizada', 'success');
      }
    });
  }

  toggleActivo(usuario: Usuario): void {
    this.service.toggleUsuarioActivo(usuario.id).subscribe({
      next: () => {
        this.loadData();
        this.showMessage(`Usuario ${usuario.activo ? 'desactivado' : 'activado'}`, 'success');
      },
      error: (err) => {
        console.error('[UsuarioList] Error toggle activo:', err);
        this.showMessage('Error al cambiar estado', 'error');
      }
    });
  }

  async deleteItem(usuario: Usuario): Promise<void> {
    if (await confirmar(`¿Desactivar el usuario "${usuario.nombreCompleto}"?\n\nEl usuario no será eliminado, solo desactivado.`)) {
      this.service.eliminarUsuario(usuario.id).subscribe({
        next: () => {
          this.loadData();
          this.showMessage('Usuario desactivado', 'success');
        },
        error: (err) => {
          console.error('[UsuarioList] Error eliminando:', err);
          this.showMessage('Error al desactivar usuario', 'error');
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
