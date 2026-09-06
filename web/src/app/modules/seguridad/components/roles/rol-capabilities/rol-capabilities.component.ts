import { Component, Inject, inject, signal, computed, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatChipsModule } from '@angular/material/chips';
import { SeguridadService } from '../../../services/seguridad.service';
import { Rol, Capability } from '../../../models/seguridad.model';
import { forkJoin } from 'rxjs';

export interface RolCapabilitiesData {
  rol: Rol;
}

interface ModuloGroup {
  modulo: string;
  capabilities: (Capability & { selected: boolean })[];
  allSelected: boolean;
  someSelected: boolean;
  expanded: boolean;
}

@Component({
  selector: 'app-rol-capabilities',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    MatCheckboxModule,
    MatProgressSpinnerModule,
    MatChipsModule
  ],
  templateUrl: './rol-capabilities.component.html',
  styleUrl: './rol-capabilities.component.scss'
})
export class RolCapabilitiesComponent implements OnInit {
  private readonly service = inject(SeguridadService);

  readonly isLoading = signal(true);
  readonly isSaving = signal(false);
  readonly searchTerm = signal('');
  readonly moduloGroups = signal<ModuloGroup[]>([]);

  readonly filteredGroups = computed(() => {
    const groups = this.moduloGroups();
    const search = this.searchTerm().toLowerCase().trim();
    if (!search) return groups;

    return groups.map(group => ({
      ...group,
      capabilities: group.capabilities.filter(c =>
        c.nombre.toLowerCase().includes(search) ||
        c.descripcion.toLowerCase().includes(search)
      )
    })).filter(g => g.capabilities.length > 0);
  });

  readonly selectedCount = computed(() => {
    return this.moduloGroups().reduce((acc, group) =>
      acc + group.capabilities.filter(c => c.selected).length, 0
    );
  });

  constructor(
    public dialogRef: MatDialogRef<RolCapabilitiesComponent>,
    @Inject(MAT_DIALOG_DATA) public data: RolCapabilitiesData
  ) {}

  ngOnInit(): void {
    this.loadData();
  }

  private loadData(): void {
    this.isLoading.set(true);

    forkJoin({
      capabilities: this.service.getCapabilities(),
      rolCapabilities: this.service.getRolCapabilities(this.data.rol.id)
    }).subscribe({
      next: ({ capabilities, rolCapabilities }) => {
        const selectedIds = new Set(rolCapabilities.map(c => c.id));

        // Agrupar por módulo
        const groupsMap = new Map<string, (Capability & { selected: boolean })[]>();

        capabilities.forEach(cap => {
          const capWithSelection = { ...cap, selected: selectedIds.has(cap.id) };
          const modulo = cap.modulo || 'General';

          if (!groupsMap.has(modulo)) {
            groupsMap.set(modulo, []);
          }
          groupsMap.get(modulo)!.push(capWithSelection);
        });

        // Convertir a array ordenado
        const groups: ModuloGroup[] = Array.from(groupsMap.entries())
          .sort((a, b) => a[0].localeCompare(b[0]))
          .map(([modulo, caps]) => ({
            modulo,
            capabilities: caps.sort((a, b) => a.nombre.localeCompare(b.nombre)),
            allSelected: caps.every(c => c.selected),
            someSelected: caps.some(c => c.selected) && !caps.every(c => c.selected),
            expanded: caps.some(c => c.selected) // Expandir si tiene selecciones
          }));

        this.moduloGroups.set(groups);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('[RolCapabilities] Error cargando datos:', err);
        this.isLoading.set(false);
      }
    });
  }

  toggleModulo(group: ModuloGroup): void {
    const newValue = !group.allSelected;
    group.capabilities.forEach(c => c.selected = newValue);
    this.updateGroupState(group);
  }

  toggleCapability(group: ModuloGroup, cap: Capability & { selected: boolean }): void {
    cap.selected = !cap.selected;
    this.updateGroupState(group);
  }

  toggleExpand(group: ModuloGroup): void {
    group.expanded = !group.expanded;
  }

  getSelectedCount(group: ModuloGroup): number {
    return group.capabilities.filter(c => c.selected).length;
  }

  private updateGroupState(group: ModuloGroup): void {
    group.allSelected = group.capabilities.every(c => c.selected);
    group.someSelected = group.capabilities.some(c => c.selected) && !group.allSelected;
    // Forzar actualización del signal
    this.moduloGroups.set([...this.moduloGroups()]);
  }

  onSave(): void {
    this.isSaving.set(true);

    const selectedIds = this.moduloGroups()
      .flatMap(g => g.capabilities)
      .filter(c => c.selected)
      .map(c => c.id);

    this.service.asignarCapabilities({ rolId: this.data.rol.id, capabilityIds: selectedIds }).subscribe({
      next: () => {
        this.isSaving.set(false);
        this.dialogRef.close(true);
      },
      error: (err: Error) => {
        console.error('[RolCapabilities] Error guardando:', err);
        this.isSaving.set(false);
      }
    });
  }

  onCancel(): void {
    this.dialogRef.close(false);
  }
}
