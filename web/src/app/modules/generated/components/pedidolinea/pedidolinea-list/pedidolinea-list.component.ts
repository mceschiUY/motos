import { Component, OnInit, OnDestroy, ViewChild, inject, signal, computed, effect } from '@angular/core';
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
import { MatTabsModule } from '@angular/material/tabs';
import { SelectionModel } from '@angular/cdk/collections';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../../../environments/environment';
import { AsistenteFormBridgeService } from '../../../../../core/services/asistente-form-bridge.service';

import { PedidoLinea } from '../../../models/pedidolinea.model';
import { PEDIDOLINEA_DESCRIPTOR } from '../../../models/pedidolinea.descriptor';
import { PedidoLineaService } from '../../../services/pedidolinea.service';
import { PedidoLineaFormComponent } from '../pedidolinea-form/pedidolinea-form.component';
import { BreadcrumbComponent, BreadcrumbItem } from '../../../../../shared/components/breadcrumb/breadcrumb.component';
import { EntityTableComponent } from '../../../../../core/components/entity-table/entity-table.component';
// Registry de vistas ARTESANALES (zona NO generada): artesanal registrada = default
// de la entidad; la tabla es el paracaídas si no hay ninguna o si el import falla.
import { vistasArtesanalesDe, VistaArtesanal } from '../../../../artesanal/artesanal.registry';

// Tipos para vistas ('artesanal:{key}' = vista artesanal del registry)
type ViewMode = 'table' | 'cards' | 'timeline' | 'kanban' | 'calendario' | 'gantt' | 'master-detail' | 'with-relations' | `artesanal:${string}`;
type DensityMode = 'comfortable' | 'compact';

interface ColumnConfig {
  key: string;
  label: string;
  visible: boolean;
  sortable: boolean;
}

// Interface para Timeline Groups
interface TimelineGroup {
  periodo: string;
  label: string;
  items: PedidoLinea[];
  count: number;
}

// Interface para navegación a relaciones hasMany
interface HasManyRelation {
  entityName: string;
  entityLabel: string;
  icon: string;
  route: string;
  fkParam: string;
}

@Component({
  selector: 'app-pedidolinea-list',
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
    MatTabsModule,
    BreadcrumbComponent,
    EntityTableComponent,
    RouterModule
  ],
  templateUrl: './pedidolinea-list.component.html',
  styleUrl: './pedidolinea-list.component.scss'
})
export class PedidoLineaListComponent implements OnInit, OnDestroy {
  private readonly service = inject(PedidoLineaService);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly http = inject(HttpClient);
  private readonly puenteVoz = inject(AsistenteFormBridgeService);

  // ═══════════════════════════════════════════════════════════════════════════
  // RELACIONES HASMANY - Entidades hijas para navegación
  // ═══════════════════════════════════════════════════════════════════════════
  readonly hasManyRelations: HasManyRelation[] = [];

  // ═══════════════════════════════════════════════════════════════════════════
  // SIGNALS - Estado Reactivo
  // ═══════════════════════════════════════════════════════════════════════════
  readonly isLoading = signal<boolean>(false);
  readonly items = signal<PedidoLinea[]>([]);
  readonly searchTerm = signal<string>('');
  readonly densityMode = signal<DensityMode>('comfortable');
  readonly viewMode = signal<ViewMode>(this.loadViewMode());
  readonly selectedItem = signal<PedidoLinea | null>(null);

  // Relations state
  private readonly _relatedItemsCache = signal<Record<string, any[]>>({} as Record<string, any[]>);
  readonly isLoadingRelation = signal<boolean>(false);
  readonly relationError = signal<string>('');

  // Vistas disponibles para esta entidad
  readonly availableViews = ['table', 'cards', 'master-detail'];

  // ── VISTAS ARTESANALES (modules/artesanal — zona NO generada) ──
  // artesanal registrada = default de la entidad; tabla es el paracaídas.
  readonly artesanales: VistaArtesanal[] = vistasArtesanalesDe('pedidolinea');
  readonly artesanalComponent = signal<any>(null);
  readonly artesanalCargando = signal<boolean>(false);

  // ── A1.5: tabla genérica del core — el shell solo pasa el descriptor y adapta eventos ──
  readonly descriptor = PEDIDOLINEA_DESCRIPTOR;
  readonly columnasOcultas = computed(() =>
    this.columnConfigs()
      .filter((c: ColumnConfig) => !c.visible && c.key !== 'select' && c.key !== 'actions')
      .map((c: ColumnConfig) => c.key));

  ejecutarAccionTabla(ev: { accion: { key: string }; item: any }): void {
    // kebab → camel: 'pasar-a-aprobada' → método pasarAAprobada(item) del shell.
    const metodo = ev.accion.key.replace(/-(\w)/g, (_m: string, c: string) => c.toUpperCase());
    (this as any)[metodo]?.(ev.item);
  }

  verHijaDesdeTabla(ev: { hija: { entidad: string }; item: any }): void {
    // descriptor.hijas conserva el ORDEN de hasManyRelations (misma fuente).
    const idx = (this.descriptor.hijas ?? []).findIndex(h => h.entidad === ev.hija.entidad);
    if (idx >= 0) (this as any).navigateToChild?.((this as any).hasManyRelations?.[idx], ev.item);
  }

  sincronizarSeleccion(items: any[]): void {
    this.selection.clear();
    if (items.length) this.selection.select(...items);
  }

  // Columnas configurables
  readonly columnConfigs = signal<ColumnConfig[]>([
    { key: 'select', label: 'Seleccionar', visible: true, sortable: false },
    { key: 'varianteDisplay', label: 'SKU', visible: true, sortable: true },
    { key: 'productoDisplay', label: 'Producto', visible: true, sortable: true },
    { key: 'cantidad', label: 'Cantidad', visible: true, sortable: true },
    { key: 'precioUnitarioUsd', label: 'Precio US$', visible: true, sortable: true },
    { key: 'subtotalUsd', label: 'Subtotal US$', visible: true, sortable: true },
    { key: 'actions', label: 'Acciones', visible: true, sortable: false }
  ]);

  // ═══════════════════════════════════════════════════════════════════════════
  // COMPUTED SIGNALS
  // ═══════════════════════════════════════════════════════════════════════════
  readonly displayedColumns = computed(() =>
    this.columnConfigs().filter((col: ColumnConfig) => col.visible).map((col: ColumnConfig) => col.key)
  );

  readonly filteredItems = computed(() => {
    const items = this.items();
    const search = this.searchTerm().toLowerCase().trim();
    if (!search) return items;
    return items.filter((item: PedidoLinea) => JSON.stringify(item).toLowerCase().includes(search));
  });

  readonly totalCount = computed(() => this.items().length);

  // Timeline grouping por fecha
  readonly gruposPorFecha = computed<TimelineGroup[]>(() => {
    const items = this.filteredItems();
    const grupos = new Map<string, PedidoLinea[]>();

    items.forEach((item: PedidoLinea) => {
      const fecha = (item as any).fechaCreacion ? new Date((item as any).fechaCreacion) : new Date();
      const periodo = `${fecha.getFullYear()}-${String(fecha.getMonth() + 1).padStart(2, '0')}`;
      if (!grupos.has(periodo)) {
        grupos.set(periodo, []);
      }
      grupos.get(periodo)!.push(item);
    });

    return Array.from(grupos.entries())
      .sort((a, b) => b[0].localeCompare(a[0]))
      .map(([periodo, items]) => {
        const [year, month] = periodo.split('-');
        const monthNames = ['Enero', 'Febrero', 'Marzo', 'Abril', 'Mayo', 'Junio',
                           'Julio', 'Agosto', 'Septiembre', 'Octubre', 'Noviembre', 'Diciembre'];
        return {
          periodo,
          label: `${monthNames[parseInt(month) - 1]} ${year}`,
          items: items.sort((a, b) => {
            const dateA = (a as any).fechaCreacion ? new Date((a as any).fechaCreacion).getTime() : 0;
            const dateB = (b as any).fechaCreacion ? new Date((b as any).fechaCreacion).getTime() : 0;
            return dateB - dateA;
          }),
          count: items.length
        };
      });
  });

  // ═══════════════════════════════════════════════════════════════════════════
  // PROPIEDADES
  // ═══════════════════════════════════════════════════════════════════════════
  isStandalone = false;
  fabMenuExpanded = false;
  dataSource = new MatTableDataSource<PedidoLinea>([]);
  selection = new SelectionModel<PedidoLinea>(true, []);
  readonly skeletonRows = Array(5).fill(0);

  breadcrumbItems: BreadcrumbItem[] = [
    { label: 'Inicio', route: '/', icon: 'home' },
    { label: 'PedidoLinea', icon: 'list_alt' }
  ];

  // Parámetros de navegación entre entidades
  
  filterByPedidoId: number | null = null;
  filterContext: string = '';

  // Paginator/Sort por setter: la tabla vive dentro de un @if (viewMode), así que
  // estos ViewChild aparecen y desaparecen — el setter re-cablea en cada render.
  @ViewChild(MatPaginator) set paginatorRef(p: MatPaginator) { if (p) this.dataSource.paginator = p; }
  @ViewChild(MatSort) set sortRef(s: MatSort) { if (s) this.dataSource.sort = s; }

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

    // Cargar items relacionados cuando cambia selección o tab
    effect(() => {
      const item = this.selectedItem();
      const tab = this.activeRelationTab();
      if (item && tab) {
        this.loadRelatedItems(tab, item.id);
      }
    }, { allowSignalWrites: true });
  }

  ngOnInit(): void {
    this.isStandalone = this.route.snapshot.data['standalone'] === true;

    // Leer queryParams para filtrado por entidad padre
    this.route.queryParams.subscribe((params: any) => {
      if (params['pedidoId']) {
        this.filterByPedidoId = +params['pedidoId'];
        this.filterContext = `Pedido #${this.filterByPedidoId}`;
        // Actualizar breadcrumb para mostrar contexto
        this.breadcrumbItems = [
          { label: 'Inicio', route: '/', icon: 'home' },
          { label: 'Pedido', route: '/pedido', icon: 'receipt_long' },
          { label: `Líneas del pedido #${this.filterByPedidoId}`, icon: 'list' }
        ];
      }

      this.loadData();
    });

    // Puente del asistente de voz: el alta por conversación abre ESTE form
    this.puenteVoz.registrarAbridor('pedidolinea', () => this.openForm());
  }

  ngOnDestroy(): void {
    this.puenteVoz.quitarAbridor('pedidolinea');
  }

  // ═══════════════════════════════════════════════════════════════════════════
  // VIEW SWITCHER
  // ═══════════════════════════════════════════════════════════════════════════
  private loadViewMode(): ViewMode {
    // clave versionada: un default nuevo del generador no queda tapado por uno guardado viejo
    const artesanales = vistasArtesanalesDe('pedidolinea');
    const validas: string[] = ['table', 'cards', 'master-detail', ...artesanales.map(v => 'artesanal:' + v.key)];
    const saved = localStorage.getItem('pedidolinea-view-mode-v2');
    if (saved && validas.includes(saved)) {
      return saved as ViewMode;
    }
    // artesanal registrada = default de la entidad; tabla es el paracaídas
    if (artesanales.length > 0) {
      return ('artesanal:' + artesanales[0].key) as ViewMode;
    }
    return 'table' as ViewMode;
  }

  setViewMode(mode: string): void {
    this.viewMode.set(mode as ViewMode);
    localStorage.setItem('pedidolinea-view-mode-v2', mode);
  }

  // Resuelve el componente artesanal (Promise del registry). Roto ⇒ tabla, sin romper.
  private cargarArtesanal(key: string): void {
    const vista = this.artesanales.find(v => v.key === key);
    if (!vista) {
      // clave guardada/registrada que ya no existe: tabla es el paracaídas
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
        console.warn('[PedidoLineaList] Vista artesanal «' + key + '» rota — caigo a tabla (paracaídas):', err);
        this.artesanalCargando.set(false);
        this.setViewMode('table');
      });
  }

  getViewIcon(view: string): string {
    const icons: Record<string, string> = {
      'table': 'table_rows',
      'cards': 'grid_view',
      'timeline': 'timeline',
      'kanban': 'view_kanban',
      'calendario': 'calendar_month',
      'gantt': 'view_timeline',
      'master-detail': 'view_sidebar',
      'with-relations': 'account_tree'
    };
    return icons[view] || 'view_list';
  }

  getViewLabel(view: string): string {
    const labels: Record<string, string> = {
      'table': 'Tabla',
      'cards': 'Tarjetas',
      'timeline': 'Timeline',
      'kanban': 'Tablero',
      'calendario': 'Calendario',
      'gantt': 'Línea de tiempo',
      'master-detail': 'Dividido',
      'with-relations': 'Con Relaciones'
    };
    return labels[view] || view;
  }

  // ═══════════════════════════════════════════════════════════════════════════
  // MÉTODOS DE DATOS
  // ═══════════════════════════════════════════════════════════════════════════
  loadData(): void {
    this.isLoading.set(true);
    this.selection.clear();

    this.service.getAll().subscribe({
      next: (data: PedidoLinea[]) => {
        // Filtrar por FKs si están definidos
        let filteredData = data;
        if (this.filterByPedidoId) {
          filteredData = filteredData.filter((item: any) => item.pedidoId === this.filterByPedidoId);
        }

        this.items.set(filteredData);
        this.isLoading.set(false);
      },
      error: (err: unknown) => {
        console.error('[PedidoLineaList] Error cargando datos:', err);
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

  toggleRow(row: PedidoLinea): void {
    this.selection.toggle(row);
  }

  clearSelection(): void {
    this.selection.clear();
  }

  selectItem(item: PedidoLinea): void {
    const previousId = this.selectedItem()?.id;
    this.selectedItem.set(item);
    if (previousId !== item.id) {
      this._relatedItemsCache.set({} as Record<string, any[]>);
      this.relationError.set('');
    }
  }

  // ═══════════════════════════════════════════════════════════════════════════
  // UI
  // ═══════════════════════════════════════════════════════════════════════════
  toggleDensity(): void {
    this.densityMode.update((mode: DensityMode) => mode === 'comfortable' ? 'compact' : 'comfortable');
  }

  toggleColumnVisibility(columnKey: string): void {
    this.columnConfigs.update((configs: ColumnConfig[]) =>
      configs.map((col: ColumnConfig) => col.key === columnKey ? { ...col, visible: !col.visible } : col)
    );
  }

  getToggleableColumns(): ColumnConfig[] {
    return this.columnConfigs().filter((c: ColumnConfig) => !['select', 'actions'].includes(c.key));
  }

  // ═══════════════════════════════════════════════════════════════════════════
  // CRUD
  // ═══════════════════════════════════════════════════════════════════════════
  openForm(item?: PedidoLinea): void {
    // Al crear desde una lista filtrada por un padre, el FK del padre ya viene dado por contexto.
    const contextoFk = this.filterByPedidoId ? { campo: 'pedidoId', valor: this.filterByPedidoId } : null;
    const dialogRef = this.dialog.open(PedidoLineaFormComponent, {
      width: '600px',
      maxWidth: '95vw',
      data: { item, mode: item ? 'edit' : 'create', contextoFk },
      panelClass: 'crm-dialog',
      autoFocus: true,
      disableClose: false
    });

    dialogRef.afterClosed().subscribe((result: any) => {
      if (result) {
        this.loadData();
        const message = item ? 'Registro actualizado' : 'Registro creado';
        this.showMessage(message, 'success');
      }
    });
  }

  async deleteItem(item: PedidoLinea): Promise<void> {
    if (await (window as any).confirmar('¿Eliminar este registro?\\n\\nEsta acción no se puede deshacer.')) {
      this.service.delete(item.id).subscribe({
        next: () => {
          this.loadData();
          this.showMessage('Registro eliminado', 'success');
        },
        error: (err: unknown) => {
          console.error('[PedidoLineaList] Error eliminando:', err);
          this.showMessage('Error al eliminar registro', 'error');
        }
      });
    }
  }

  async deleteSelected(): Promise<void> {
    const count = this.selection.selected.length;
    if (await (window as any).confirmar(`¿Eliminar ${count} registro(s)?\\n\\nEsta acción no se puede deshacer.`)) {
      const deleteObservables = this.selection.selected.map((item: PedidoLinea) => this.service.delete(item.id));
      import('rxjs').then(({ forkJoin }) => {
        forkJoin(deleteObservables).subscribe({
          next: () => {
            this.selection.clear();
            this.loadData();
            this.showMessage(`${count} registro(s) eliminado(s)`, 'success');
          },
          error: (err: unknown) => {
            console.error('[PedidoLineaList] Error eliminando múltiples:', err);
            this.loadData();
            this.showMessage('Error al eliminar algunos registros', 'error');
          }
        });
      });
    }
  }

  // ═══════════════════════════════════════════════════════════════════════════
  // EXPORTACIÓN
  // ═══════════════════════════════════════════════════════════════════════════
  exportSelected(): void {
    const data = this.selection.selected;
    if (data.length === 0) return;
    this.exportData(data, 'pedidolinea_seleccionados');
    this.showMessage(`${data.length} registro(s) exportado(s)`, 'success');
  }

  exportAll(): void {
    const data = this.items();
    if (data.length === 0) {
      this.showMessage('No hay datos para exportar', 'error');
      return;
    }
    this.exportData(data, 'pedidolinea_todos');
    this.showMessage('Archivo CSV exportado', 'success');
  }

  private exportData(data: PedidoLinea[], filename: string): void {
    const headers = Object.keys(data[0] || {});
    const rows = data.map((item: any) => headers.map((h: string) => (item as any)[h]?.toString() || ''));
    const csvContent = [headers, ...rows].map(row => row.map(cell => '"' + cell + '"').join(',')).join('\\n');
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

  openDocuments(item: PedidoLinea): void {
    this.router.navigate(['/documento'], {
      queryParams: {
        relacionId: item.id,
        relacionNombre: 'PedidoLinea'
      }
    });
  }

  verFicha(item: PedidoLinea): void {
    this.router.navigate(['/pedidolinea', item.id]);
  }

  private showMessage(message: string, type: 'success' | 'error'): void {
    this.snackBar.open(message, 'Cerrar', {
      duration: 4000,
      horizontalPosition: 'end',
      verticalPosition: 'top',
      panelClass: type === 'error' ? 'snackbar-error' : 'snackbar-success'
    });
  }

  // ═══════════════════════════════════════════════════════════════════════════
  // HELPERS PARA TEMPLATES
  // ═══════════════════════════════════════════════════════════════════════════
  getItemTitle(item: PedidoLinea): string {
    return (item as any).nombre || (item as any).nombre || (item as any).titulo || `#${item.id}`;
  }

  getItemSubtitle(item: PedidoLinea): string {
    return (item as any).descripcion || (item as any).email || '';
  }

  getStatusClass(item: PedidoLinea): string {
    const status = (item as any).activo;
    if (status === true || status === 'activo' || status === 'Activo') return 'status-active';
    if (status === false || status === 'inactivo' || status === 'Inactivo') return 'status-inactive';
    return 'status-default';
  }

  getStatusLabel(item: PedidoLinea): string {
    const status = (item as any).activo;
    if (status === true) return 'Activo';
    if (status === false) return 'Inactivo';
    return status?.toString() || '';
  }

  // ═══════════════════════════════════════════════════════════════════════════
  // WITH-RELATIONS HELPERS
  // ═══════════════════════════════════════════════════════════════════════════
  readonly activeRelationTab = signal<string>('');

  setActiveRelationTab(tab: string): void {
    this.activeRelationTab.set(tab);
  }

  // El caché va claveado por relación + PADRE: sin el id, al cambiar de registro
  // seleccionado se mostraban los hijos del anterior (datos entreverados).
  getRelatedItems(entityName: string): any[] {
    const pid = this.selectedItem()?.id;
    if (pid === null || pid === undefined) { return []; }
    return this._relatedItemsCache()[entityName + ':' + pid] || [];
  }

  private loadRelatedItems(entityNameCamel: string, parentId: string | number): void {
    const relation = this.hasManyRelations.find(
      r => r.entityName.toLowerCase() === entityNameCamel.toLowerCase()
    );
    if (!relation) return;

    const clave = entityNameCamel + ':' + parentId;
    // Si ya está en cache PARA ESTE PADRE, no recargar
    if (this._relatedItemsCache()[clave]) return;

    this.isLoadingRelation.set(true);
    this.relationError.set('');

    const parentName = relation.fkParam.replace(/Id$/i, '').toLowerCase();
    const url = `${environment.apiUrl}/${relation.entityName}/by-${parentName}/${parentId}`;

    this.http.get<any[]>(url).subscribe({
      next: (items: any[]) => {
        this._relatedItemsCache.update((cache: Record<string, any[]>) => ({ ...cache, [clave]: items }));
        this.isLoadingRelation.set(false);
      },
      error: (err: unknown) => {
        console.error(`[PedidoLineaList] Error cargando ${relation.entityName}:`, err);
        this._relatedItemsCache.update((cache: Record<string, any[]>) => ({ ...cache, [clave]: [] }));
        this.isLoadingRelation.set(false);
        this.relationError.set(`Error al cargar ${relation.entityLabel}`);
      }
    });
  }

  retryLoadRelation(): void {
    const tab = this.activeRelationTab();
    const item = this.selectedItem();
    if (tab && item) {
      this._relatedItemsCache.update((cache: Record<string, any[]>) => {
        const updated = { ...cache };
        delete updated[tab + ':' + item.id];
        return updated;
      });
      this.relationError.set('');
      this.loadRelatedItems(tab, item.id);
    }
  }

  openRelatedForm(entityName: string): void {
    const relation = this.hasManyRelations.find(
      r => r.entityName.toLowerCase() === entityName.toLowerCase()
    );
    if (!relation) return;

    const parentId = this.selectedItem()?.id;
    if (!parentId) return;

    this.router.navigate([relation.route], {
      queryParams: { [relation.fkParam]: parentId }
    });
  }

  editRelatedItem(entityName: string, item: any): void {
    const relation = this.hasManyRelations.find(
      r => r.entityName.toLowerCase() === entityName.toLowerCase()
    );
    if (!relation) return;
    this.router.navigate([relation.route, item.id]);
  }

  async deleteRelatedItem(entityName: string, item: any): Promise<void> {
    if (!(await (window as any).confirmar('¿Eliminar este registro?\\n\\nEsta acción no se puede deshacer.'))) return;
    const relation = this.hasManyRelations.find(
      r => r.entityName.toLowerCase() === entityName.toLowerCase()
    );
    if (!relation) return;
    const parentId = this.selectedItem()?.id;
    if (!parentId) return;
    const url = `${environment.apiUrl}/${relation.entityName}/${item.id}`;
    this.http.delete(url).subscribe({
      next: () => this.loadRelatedItems(entityName, parentId),
      error: (err: unknown) => {
        console.error(`Error eliminando ${relation.entityName}:`, err);
        this.relationError.set('Error al eliminar el registro');
      }
    });
  }

  getRelationColumns(entityName: string): string[] {
    const items = this.getRelatedItems(entityName);
    if (items.length === 0) return [];
    return Object.keys(items[0]).filter(k => k !== 'id' && !k.endsWith('Id') && !k.endsWith('id'));
  }

  // ═══════════════════════════════════════════════════════════════════════════
  // NAVEGACIÓN A ENTIDADES HIJAS (HASMANY)
  // ═══════════════════════════════════════════════════════════════════════════
  /**
   * Navega a una entidad hija de forma genérica usando la información de relación
   */
  navigateToChild(relation: HasManyRelation, item: PedidoLinea): void {
    this.router.navigate([relation.route], {
      queryParams: { [relation.fkParam]: item.id }
    });
  }
}