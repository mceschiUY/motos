import { Component, Inject, inject, signal, computed, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { SeguridadService } from '../../../services/seguridad.service';
import { Perfil, Rol } from '../../../models/seguridad.model';
import { forkJoin } from 'rxjs';

export interface PerfilRolesData {
  perfil: Perfil;
}

@Component({
  selector: 'app-perfil-roles',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    MatCheckboxModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './perfil-roles.component.html',
  styleUrl: './perfil-roles.component.scss'
})
export class PerfilRolesComponent implements OnInit {
  private readonly service = inject(SeguridadService);

  readonly isLoading = signal(true);
  readonly isSaving = signal(false);
  readonly searchTerm = signal('');
  readonly roles = signal<(Rol & { selected: boolean })[]>([]);

  readonly filteredRoles = computed(() => {
    const items = this.roles();
    const search = this.searchTerm().toLowerCase().trim();
    if (!search) return items;
    return items.filter(r =>
      r.nombre.toLowerCase().includes(search) ||
      r.descripcion.toLowerCase().includes(search)
    );
  });

  readonly selectedCount = computed(() =>
    this.roles().filter(r => r.selected).length
  );

  readonly allSelected = computed(() =>
    this.filteredRoles().length > 0 && this.filteredRoles().every(r => r.selected)
  );

  readonly someSelected = computed(() =>
    this.filteredRoles().some(r => r.selected) && !this.filteredRoles().every(r => r.selected)
  );

  constructor(
    public dialogRef: MatDialogRef<PerfilRolesComponent>,
    @Inject(MAT_DIALOG_DATA) public data: PerfilRolesData
  ) {}

  ngOnInit(): void {
    this.loadData();
  }

  private loadData(): void {
    this.isLoading.set(true);

    forkJoin({
      roles: this.service.getRoles(),
      perfilRoles: this.service.getPerfilRoles(this.data.perfil.id)
    }).subscribe({
      next: ({ roles, perfilRoles }) => {
        const selectedIds = new Set(perfilRoles.map(r => r.id));

        const rolesWithSelection = roles
          .filter(r => r.activo)
          .map(rol => ({
            ...rol,
            selected: selectedIds.has(rol.id)
          }))
          .sort((a, b) => a.nombre.localeCompare(b.nombre));

        this.roles.set(rolesWithSelection);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('[PerfilRoles] Error cargando datos:', err);
        this.isLoading.set(false);
      }
    });
  }

  toggleRole(rol: Rol & { selected: boolean }): void {
    rol.selected = !rol.selected;
    this.roles.set([...this.roles()]);
  }

  toggleAll(): void {
    const allSelected = this.filteredRoles().every(r => r.selected);
    this.filteredRoles().forEach(r => r.selected = !allSelected);
    this.roles.set([...this.roles()]);
  }

  onSave(): void {
    this.isSaving.set(true);

    const selectedIds = this.roles()
      .filter(r => r.selected)
      .map(r => r.id);

    this.service.asignarRoles({ perfilId: this.data.perfil.id, rolIds: selectedIds }).subscribe({
      next: () => {
        this.isSaving.set(false);
        this.dialogRef.close(true);
      },
      error: (err: Error) => {
        console.error('[PerfilRoles] Error guardando:', err);
        this.isSaving.set(false);
      }
    });
  }

  onCancel(): void {
    this.dialogRef.close(false);
  }
}
