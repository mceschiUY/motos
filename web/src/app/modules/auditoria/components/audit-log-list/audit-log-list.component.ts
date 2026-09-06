import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatSortModule, Sort } from '@angular/material/sort';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatChipsModule } from '@angular/material/chips';
import { MatMenuModule } from '@angular/material/menu';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { BreadcrumbComponent, BreadcrumbItem } from '../../../../shared/components/breadcrumb/breadcrumb.component';
import { AuditoriaService } from '../../services/auditoria.service';
import { AuditLog, AuditLogFiltro, getActionLabel, getActionIcon, getActionColor } from '../../models/auditoria.model';
import { AuditLogDetailComponent } from '../audit-log-detail/audit-log-detail.component';

@Component({
  selector: 'app-audit-log-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatTableModule,
    MatSortModule,
    MatPaginatorModule,
    MatButtonModule,
    MatIconModule,
    MatDialogModule,
    MatSnackBarModule,
    MatTooltipModule,
    MatChipsModule,
    MatMenuModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatProgressSpinnerModule,
    BreadcrumbComponent
  ],
  templateUrl: './audit-log-list.component.html',
  styleUrl: './audit-log-list.component.scss'
})
export class AuditLogListComponent implements OnInit {
  private readonly service = inject(AuditoriaService);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);

  // Signals
  readonly isLoading = signal<boolean>(false);
  readonly items = signal<AuditLog[]>([]);
  readonly totalCount = signal<number>(0);
  readonly searchTerm = signal<string>('');

  // Búsqueda con debounce: sin esto, cada tecla recargaba y el skeleton
  // reemplazaba la tabla (la vista "se rompía" mientras se tipeaba).
  private readonly search$ = new Subject<string>();
  // Descarta respuestas fuera de orden (una request lenta no pisa a una nueva)
  private loadSeq = 0;

  constructor() {
    this.search$
      .pipe(debounceTime(300), distinctUntilChanged(), takeUntilDestroyed())
      .subscribe(value => {
        this.searchTerm.set(value);
        this.page.set(1);
        this.loadData();
      });
  }

  // Filtros
  readonly filtroDesde = signal<Date | null>(null);
  readonly filtroHasta = signal<Date | null>(null);
  readonly filtroAccion = signal<string>('');
  readonly filtroEntidad = signal<string>('');
  readonly filtroExito = signal<string>('');

  // Paginacion
  readonly page = signal<number>(1);
  readonly pageSize = signal<number>(25);
  readonly sortBy = signal<string>('Timestamp');
  readonly sortDesc = signal<boolean>(true);

  // Computed
  readonly totalPages = computed(() => Math.ceil(this.totalCount() / this.pageSize()));
  readonly hasFilters = computed(() =>
    this.searchTerm() !== '' ||
    this.filtroDesde() !== null ||
    this.filtroHasta() !== null ||
    this.filtroAccion() !== '' ||
    this.filtroEntidad() !== '' ||
    this.filtroExito() !== ''
  );

  // Tabla
  displayedColumns = ['timestamp', 'usuario', 'action', 'entityType', 'entityId', 'success', 'duration', 'actions'];
  dataSource = new MatTableDataSource<AuditLog>([]);
  readonly skeletonRows = Array(10).fill(0);

  // Acciones disponibles
  readonly acciones = [
    { value: 'Create', label: 'Crear' },
    { value: 'Update', label: 'Modificar' },
    { value: 'Delete', label: 'Eliminar' },
    { value: 'Read', label: 'Leer' },
    { value: 'Login', label: 'Login' },
    { value: 'Logout', label: 'Logout' },
    { value: 'LoginFailed', label: 'Login fallido' }
  ];

  breadcrumbItems: BreadcrumbItem[] = [
    { label: 'Inicio', icon: 'home' },
    { label: 'Sistema', icon: 'settings' },
    { label: 'Auditoria', icon: 'history' }
  ];

  ngOnInit(): void {
    // Establecer filtro por defecto: últimos 7 días
    const hace7Dias = new Date();
    hace7Dias.setDate(hace7Dias.getDate() - 7);
    this.filtroDesde.set(hace7Dias);
    this.filtroHasta.set(new Date());

    this.loadData();
  }

  loadData(): void {
    const seq = ++this.loadSeq;
    this.isLoading.set(true);

    const filtro: Partial<AuditLogFiltro> = {
      page: this.page(),
      pageSize: this.pageSize(),
      sortBy: this.sortBy(),
      sortDesc: this.sortDesc()
    };

    if (this.searchTerm()) filtro.search = this.searchTerm();
    if (this.filtroDesde()) filtro.desde = this.filtroDesde()!;
    if (this.filtroHasta()) filtro.hasta = this.filtroHasta()!;
    if (this.filtroAccion()) filtro.actions = [this.filtroAccion()];
    if (this.filtroEntidad()) filtro.entityType = this.filtroEntidad();
    if (this.filtroExito() !== '') filtro.success = this.filtroExito() === 'true';

    this.service.getAuditLogs(filtro).subscribe({
      next: (response) => {
        if (seq !== this.loadSeq) return; // llegó tarde: ya hay una búsqueda más nueva
        this.items.set(response.items);
        this.totalCount.set(response.totalCount);
        this.dataSource.data = response.items;
        this.isLoading.set(false);
      },
      error: (err) => {
        if (seq !== this.loadSeq) return;
        console.error('[AuditLogList] Error cargando datos:', err);
        this.isLoading.set(false);
        this.showMessage('Error al cargar registros de auditoria', 'error');
      }
    });
  }

  onSearchChange(event: Event): void {
    this.search$.next((event.target as HTMLInputElement).value);
  }

  onFiltroChange(): void {
    this.page.set(1);
    this.loadData();
  }

  clearFilters(): void {
    this.searchTerm.set('');
    this.filtroDesde.set(null);
    this.filtroHasta.set(null);
    this.filtroAccion.set('');
    this.filtroEntidad.set('');
    this.filtroExito.set('');
    this.page.set(1);
    this.loadData();
  }

  onPageChange(event: PageEvent): void {
    this.page.set(event.pageIndex + 1);
    this.pageSize.set(event.pageSize);
    this.loadData();
  }

  onSortChange(sort: Sort): void {
    if (sort.active && sort.direction) {
      this.sortBy.set(sort.active);
      this.sortDesc.set(sort.direction === 'desc');
      this.loadData();
    }
  }

  openDetail(log: AuditLog): void {
    this.dialog.open(AuditLogDetailComponent, {
      width: '700px',
      maxHeight: '90vh',
      data: { log }
    });
  }

  refresh(): void {
    this.loadData();
  }

  // Helpers para el template
  getActionLabel = getActionLabel;
  getActionIcon = getActionIcon;
  getActionColor = getActionColor;

  formatDate(date: Date | string): string {
    const d = new Date(date);
    return d.toLocaleDateString('es-AR', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
      second: '2-digit'
    });
  }

  formatDuration(ms: number | null): string {
    if (ms === null) return '-';
    if (ms < 1000) return `${ms}ms`;
    return `${(ms / 1000).toFixed(2)}s`;
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