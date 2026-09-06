import { Component, OnInit, ViewChild, inject, signal, computed, effect } from '@angular/core';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatPaginatorModule, MatPaginator } from '@angular/material/paginator';
import { MatSortModule, MatSort } from '@angular/material/sort';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatMenuModule } from '@angular/material/menu';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { SelectionModel } from '@angular/cdk/collections';

import { Conceptoexpensa } from '../../../models/conceptoexpensa.model';
import { ConceptoexpensaService } from '../../../services/conceptoexpensa.service';
import { ConceptoexpensaFormComponent } from '../conceptoexpensa-form/conceptoexpensa-form.component';
import { BreadcrumbComponent, BreadcrumbItem } from '../../../../../shared/components/breadcrumb/breadcrumb.component';
import { ViewToggleComponent, ViewMode } from '../../../../../shared/components/view-toggle/view-toggle.component';
import { confirmar } from '../../../../../shared/confirm/confirmar';
// Registry de vistas ARTESANALES (zona NO generada): artesanal registrada = default
// de la entidad; la tabla es el paracaídas si no hay ninguna o si el import falla.
import { vistasArtesanalesDe, VistaArtesanal } from '../../../../artesanal/artesanal.registry';

// Tipos para Smart Data Table ('artesanal:{key}' = vista artesanal del registry)
type VistaActiva = ViewMode | `artesanal:${string}`;
type DensityMode = 'comfortable' | 'compact';

interface ColumnConfig {
  key: string;
  label: string;
  visible: boolean;
  sortable: boolean;
}

@Component({
  selector: 'app-conceptoexpensa-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatDialogModule,
    MatSnackBarModule,
    MatTooltipModule,
    MatProgressSpinnerModule,
    MatCheckboxModule,
    MatMenuModule,
    MatButtonToggleModule,
    BreadcrumbComponent,
    ViewToggleComponent,
    RouterModule
  ],
  templateUrl: './conceptoexpensa-list.component.html',
  styleUrl: './conceptoexpensa-list.component.scss'
})
export class ConceptoexpensaListComponent implements OnInit {
  private readonly service = inject(ConceptoexpensaService);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  // ═══════════════════════════════════════════════════════════════════════════
  // SIGNALS - Estado Reactivo
  // ═══════════════════════════════════════════════════════════════════════════
  readonly isLoading = signal<boolean>(false);
  readonly items = signal<Conceptoexpensa[]>([]);
  readonly searchTerm = signal<string>('');
  readonly densityMode = signal<DensityMode>('comfortable');
  readonly viewMode = signal<VistaActiva>(this.vistaInicial());

  // ── VISTAS ARTESANALES (modules/artesanal — zona NO generada) ──
  // artesanal registrada = default de la entidad; tabla es el paracaídas.
  readonly artesanales: VistaArtesanal[] = vistasArtesanalesDe('conceptoexpensa');
  readonly artesanalComponent = signal<any>(null);
  readonly artesanalCargando = signal<boolean>(false);

  // Columnas configurables
  readonly columnConfigs = signal<ColumnConfig[]>([
    { key: 'select', label: 'Seleccionar', visible: true, sortable: false },
    { key: 'nombre', label: 'Nombre', visible: true, sortable: true },
    { key: 'descripcion', label: 'Descripcion', visible: true, sortable: true },
    { key: 'tipo', label: 'Tipo', visible: true, sortable: true },
    { key: 'activo', label: 'Activo', visible: true, sortable: true },
    { key: 'actions', label: 'Acciones', visible: true, sortable: false }
  ]);

  // ═══════════════════════════════════════════════════════════════════════════
  // COMPUTED SIGNALS
  // ═══════════════════════════════════════════════════════════════════════════
  readonly displayedColumns = computed(() =>
    this.columnConfigs().filter(col => col.visible).map(col => col.key)
  );

  readonly filteredItems = computed(() => {
    const items = this.items();
    const search = this.searchTerm().toLowerCase().trim();
    if (!search) return items;
    return items.filter(item => JSON.stringify(item).toLowerCase().includes(search));
  });

  readonly totalCount = computed(() => this.items().length);

  // ═══════════════════════════════════════════════════════════════════════════
  // PROPIEDADES
  // ═══════════════════════════════════════════════════════════════════════════
  isStandalone = false;
  fabMenuExpanded = false;
  dataSource = new MatTableDataSource<Conceptoexpensa>([]);
  selection = new SelectionModel<Conceptoexpensa>(true, []);
  readonly skeletonRows = Array(5).fill(0);

  breadcrumbItems: BreadcrumbItem[] = [
    { label: 'Inicio', route: '/', icon: 'home' },
    { label: 'Conceptoexpensa', icon: 'list_alt' }
  ];

  // Parámetros de navegación entre entidades
  filterContext: string = '';
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  // ═══════════════════════════════════════════════════════════════════════════
  // LIFECYCLE
  // ═══════════════════════════════════════════════════════════════════════════
  constructor() {
    effect(() => {
      this.dataSource.data = this.filteredItems();
    });

    // Vista artesanal activa ⇒ resolver su loadComponent() lazy. Si el import
    // falla (componente roto), se cae a 'table' sin romper: tabla = paracaídas.
    effect(() => {
      const modo = this.viewMode();
      if (modo.startsWith('artesanal:')) {
        this.cargarArtesanal(modo.slice('artesanal:'.length));
      } else {
        this.artesanalComponent.set(null);
      }
    }, { allowSignalWrites: true });
  }

  // artesanal registrada = default de la entidad; tabla es el paracaídas
  private vistaInicial(): VistaActiva {
    const artesanales = vistasArtesanalesDe('conceptoexpensa');
    if (artesanales.length > 0) {
      return ('artesanal:' + artesanales[0].key) as VistaActiva;
    }
    return 'table';
  }

  setViewMode(mode: string): void {
    this.viewMode.set(mode as VistaActiva);
  }

  // Resuelve el componente artesanal (Promise del registry). Roto ⇒ tabla, sin romper.
  private cargarArtesanal(key: string): void {
    const vista = this.artesanales.find(v => v.key === key);
    if (!vista) {
      // clave registrada que ya no existe: tabla es el paracaídas
      this.setViewMode('table');
      return;
    }
    this.artesanalCargando.set(true);
    this.artesanalComponent.set(null);
    vista.loadComponent()
      .then((comp: any) => {
        this.artesanalComponent.set(comp);
        this.artesanalCargando.set(false);
      })
      .catch((err: unknown) => {
        console.warn('[ConceptoexpensaList] Vista artesanal «' + key + '» rota — caigo a tabla (paracaídas):', err);
        this.artesanalCargando.set(false);
        this.setViewMode('table');
      });
  }

  ngOnInit(): void {
    this.isStandalone = this.route.snapshot.data['standalone'] === true;

    // Leer parámetros de navegación entre entidades

    this.loadData();
  }

  ngAfterViewInit(): void {
    this.dataSource.paginator = this.paginator;
    this.dataSource.sort = this.sort;
  }

  // ═══════════════════════════════════════════════════════════════════════════
  // MÉTODOS DE DATOS
  // ═══════════════════════════════════════════════════════════════════════════
  loadData(): void {
    this.isLoading.set(true);
    this.selection.clear();

    // Determinar qué método llamar según los filtros
    let dataObservable;
    dataObservable = this.service.getAll();

    dataObservable.subscribe({
      next: (data) => {
        this.items.set(data);
        this.isLoading.set(false);
      },
      error: (err: unknown) => {
        console.error('[ConceptoexpensaList] Error cargando datos:', err);
        this.isLoading.set(false);
        this.showMessage('Error al cargar datos', 'error');
      }
    });

  }

  onSearchChange(event: Event): void {
    const value = (event.target as HTMLInputElement).value;
    this.searchTerm.set(value);
    if (this.dataSource.paginator) {
      this.dataSource.paginator.firstPage();
    }
  }

  // ═══════════════════════════════════════════════════════════════════════════
  // SELECCIÓN
  // ═══════════════════════════════════════════════════════════════════════════
  isAllSelected(): boolean {
    const numSelected = this.selection.selected.length;
    const numRows = this.filteredItems().length;
    return numSelected === numRows && numRows > 0;
  }

  toggleAllRows(): void {
    if (this.isAllSelected()) {
      this.selection.clear();
    } else {
      this.selection.select(...this.filteredItems());
    }
  }

  toggleRow(row: Conceptoexpensa): void {
    this.selection.toggle(row);
  }

  clearSelection(): void {
    this.selection.clear();
  }

  // ═══════════════════════════════════════════════════════════════════════════
  // UI
  // ═══════════════════════════════════════════════════════════════════════════
  toggleDensity(): void {
    this.densityMode.update(mode => mode === 'comfortable' ? 'compact' : 'comfortable');
  }

  toggleColumnVisibility(columnKey: string): void {
    this.columnConfigs.update(configs =>
      configs.map(col => col.key === columnKey ? { ...col, visible: !col.visible } : col)
    );
  }

  getToggleableColumns(): ColumnConfig[] {
    return this.columnConfigs().filter(c => !['select', 'actions'].includes(c.key));
  }

  onViewModeChange(mode: ViewMode): void {
    this.viewMode.set(mode);
  }

  // El toggle table/cards solo entiende ViewMode: con una artesanal activa se le muestra 'table'.
  vistaParaToggle(): ViewMode {
    const modo = this.viewMode();
    return modo === 'cards' ? 'cards' : 'table';
  }

  // ═══════════════════════════════════════════════════════════════════════════
  // CRUD
  // ═══════════════════════════════════════════════════════════════════════════
  openForm(item?: Conceptoexpensa): void {
    const dialogRef = this.dialog.open(ConceptoexpensaFormComponent, {
      width: '600px',
      maxWidth: '95vw',
      data: { item, mode: item ? 'edit' : 'create' },
      panelClass: 'crm-dialog',
      autoFocus: true,
      disableClose: false
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.loadData();
        const message = item ? 'Registro actualizado' : 'Registro creado';
        this.showMessage(message, 'success');
      }
    });
  }

  async deleteItem(item: Conceptoexpensa): Promise<void> {
    if (await confirmar('¿Eliminar este registro?\\n\\nEsta acción no se puede deshacer.')) {
      this.service.delete(item.id).subscribe({
        next: () => {
          this.loadData();
          this.showMessage('Registro eliminado', 'success');
        },
        error: (err: unknown) => {
          console.error('[ConceptoexpensaList] Error eliminando:', err);
          this.showMessage('Error al eliminar registro', 'error');
        }
      });
    }
  }

  async deleteSelected(): Promise<void> {
    const count = this.selection.selected.length;
    if (await confirmar(`¿Eliminar ${count} registro(s)?\\n\\nEsta acción no se puede deshacer.`)) {
      const deleteObservables = this.selection.selected.map(item => this.service.delete(item.id));
      import('rxjs').then(({ forkJoin }) => {
        forkJoin(deleteObservables).subscribe({
          next: () => {
            this.selection.clear();
            this.loadData();
            this.showMessage(`${count} registro(s) eliminado(s)`, 'success');
          },
          error: (err: unknown) => {
            console.error('[ConceptoexpensaList] Error eliminando múltiples:', err);
            this.loadData();
            this.showMessage('Error al eliminar algunos registros', 'error');
          }
        });
      });
    }
  }

  exportSelected(): void {
    const data = this.selection.selected;
    if (data.length === 0) return;
    this.exportData(data, 'conceptoexpensa_seleccionados');
    this.showMessage(`${data.length} registro(s) exportado(s)`, 'success');
  }

  exportAll(): void {
    const data = this.items();
    if (data.length === 0) {
      this.showMessage('No hay datos para exportar', 'error');
      return;
    }
    this.exportData(data, 'conceptoexpensa_todos');
    this.showMessage('Archivo CSV exportado', 'success');
  }

  private exportData(data: Conceptoexpensa[], filename: string): void {
    const headers = Object.keys(data[0] || {});
    const rows = data.map(item => headers.map(h => (item as any)[h]?.toString() || ''));
    const csvContent = [headers, ...rows].map(row => row.map(cell => '"' + cell + '"').join(',')).join('\n');
    const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
    const link = document.createElement('a');
    link.href = URL.createObjectURL(blob);
    link.download = filename + '_' + new Date().toISOString().split('T')[0] + '.csv';
    link.click();
  }

  // ═══════════════════════════════════════════════════════════════════════════
  // NAVEGACIÓN
  // ═══════════════════════════════════════════════════════════════════════════
  goBack(): void {
    window.close();
    setTimeout(() => this.router.navigate(['/kosmos']), 100);
  }

  toggleFabMenu(): void {
    this.fabMenuExpanded = !this.fabMenuExpanded;
  }

  /**
   * Navega a la página de documentos para un registro
   */
  openDocuments(item: Conceptoexpensa): void {
    this.router.navigate(['/documento'], {
      queryParams: {
        relacionId: item.id,
        relacionNombre: 'Conceptoexpensa'
      }
    });
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
