import { Component, OnInit, Input, inject, signal, computed, Injector } from '@angular/core';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatSortModule, MatSort } from '@angular/material/sort';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog, MatDialogModule, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { SelectionModel } from '@angular/cdk/collections';

export interface DocumentoListDialogData {
  relacionId?: number;
  relacionNombre?: string;
  embedded?: boolean;
}

import { Documento } from '../../../models/documento.model';
import { DocumentoService } from '../../../services/documento.service';
import { DocumentoUploadComponent } from '../documento-upload/documento-upload.component';
import { DocumentoViewerComponent } from '../documento-viewer/documento-viewer.component';
import { confirmar } from '../../../../../shared/confirm/confirmar';
import { BreadcrumbComponent, BreadcrumbItem } from '../../../../../shared/components/breadcrumb/breadcrumb.component';

@Component({
  selector: 'app-documento-list',
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
    MatProgressSpinnerModule,
    MatCheckboxModule,
    BreadcrumbComponent,
    RouterModule
  ],
  templateUrl: './documento-list.component.html',
  styleUrl: './documento-list.component.scss'
})
export class DocumentoListComponent implements OnInit {
  private readonly service = inject(DocumentoService);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly injector = inject(Injector);

  // Inputs para integracion universal (cuando se usa como componente)
  @Input() relacionId?: number;
  @Input() relacionNombre?: string;
  @Input() embedded: boolean = false;

  // Datos del dialogo (cuando se abre como dialogo)
  private dialogData: DocumentoListDialogData | null = null;

  // Signals
  readonly isLoading = signal<boolean>(false);
  readonly items = signal<Documento[]>([]);
  readonly searchTerm = signal<string>('');
  fabMenuExpanded = false;

  // Computed
  readonly filteredItems = computed(() => {
    const items = this.items();
    const search = this.searchTerm().toLowerCase().trim();
    if (!search) return items;
    return items.filter(item =>
      item.nombre.toLowerCase().includes(search) ||
      item.extension.toLowerCase().includes(search)
    );
  });

  readonly totalCount = computed(() => this.items().length);

  // Propiedades
  displayedColumns = ['select', 'icon', 'nombre', 'extension', 'fechaCarga', 'actions'];
  dataSource = new MatTableDataSource<Documento>([]);
  selection = new SelectionModel<Documento>(true, []);
  readonly skeletonRows = Array(5).fill(0);

  breadcrumbItems: BreadcrumbItem[] = [
    { label: 'Inicio', icon: 'home' },
    { label: 'Documentos', icon: 'folder' }
  ];

  ngOnInit(): void {
    // Intentar obtener datos del dialogo de forma segura
    try {
      this.dialogData = this.injector.get<DocumentoListDialogData | null>(MAT_DIALOG_DATA, null, { optional: true });
    } catch {
      this.dialogData = null;
    }

    // Prioridad 1: Datos del dialogo (MAT_DIALOG_DATA)
    if (this.dialogData) {
      this.relacionId = this.dialogData.relacionId;
      this.relacionNombre = this.dialogData.relacionNombre;
      this.embedded = this.dialogData.embedded ?? true;
      this.updateBreadcrumb();
      this.loadData();
      return;
    }

    // Prioridad 2: Inputs directos (cuando se usa como componente)
    if (this.relacionId) {
      this.updateBreadcrumb();
      this.loadData();
      return;
    }

    // Prioridad 3: Query params (navegacion por URL)
    this.route.queryParams.subscribe(params => {
      if (params['relacionId']) {
        this.relacionId = +params['relacionId'];
        this.relacionNombre = params['relacionNombre'] || '';
        this.updateBreadcrumb();
      }
      this.loadData();
    });
  }

  private updateBreadcrumb(): void {
    if (this.relacionNombre && this.relacionId) {
      this.breadcrumbItems = [
        { label: 'Inicio', icon: 'home' },
        { label: this.relacionNombre, icon: 'list_alt' },
        { label: `Documentos (#${this.relacionId})`, icon: 'folder' }
      ];
    }
  }

  loadData(): void {
    this.isLoading.set(true);
    this.selection.clear();

    const observable = this.relacionId && this.relacionNombre
      ? this.service.getByRelacion(this.relacionId, this.relacionNombre)
      : this.service.getAll();

    observable.subscribe({
      next: (data) => {
        this.items.set(data);
        this.dataSource.data = data;
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('[DocumentoList] Error cargando datos:', err);
        this.isLoading.set(false);
        this.showMessage('Error al cargar documentos', 'error');
      }
    });
  }

  onSearchChange(event: Event): void {
    const value = (event.target as HTMLInputElement).value;
    this.searchTerm.set(value);
    this.dataSource.data = this.filteredItems();
  }

  // Seleccion
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

  toggleRow(row: Documento): void {
    this.selection.toggle(row);
  }

  clearSelection(): void {
    this.selection.clear();
  }

  // Iconos y helpers
  getFileIcon(extension: string): string {
    return this.service.getFileIcon(extension);
  }

  canPreview(extension: string): boolean {
    return this.service.canPreview(extension);
  }

  // Acciones
  openUpload(): void {
    const dialogRef = this.dialog.open(DocumentoUploadComponent, {
      width: '500px',
      data: {
        relacionId: this.relacionId || 0,
        relacionNombre: this.relacionNombre || 'General'
      }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.loadData();
        this.showMessage('Documento subido correctamente', 'success');
      }
    });
  }

  openViewer(item: Documento): void {
    this.dialog.open(DocumentoViewerComponent, {
      width: '90vw',
      height: '90vh',
      maxWidth: '1200px',
      data: item
    });
  }

  download(item: Documento): void {
    const url = this.service.getDownloadUrl(item.id);
    window.open(url, '_blank');
  }

  async deleteItem(item: Documento): Promise<void> {
    if (await confirmar(`Eliminar "${item.nombre}${item.extension}"?\n\nEsta accion no se puede deshacer.`)) {
      this.service.delete(item.id).subscribe({
        next: () => {
          this.loadData();
          this.showMessage('Documento eliminado', 'success');
        },
        error: (err) => {
          console.error('[DocumentoList] Error eliminando:', err);
          this.showMessage('Error al eliminar documento', 'error');
        }
      });
    }
  }

  async deleteSelected(): Promise<void> {
    const count = this.selection.selected.length;
    if (await confirmar(`Eliminar ${count} documento(s)?\n\nEsta accion no se puede deshacer.`)) {
      const deletePromises = this.selection.selected.map(item =>
        this.service.delete(item.id).toPromise()
      );

      Promise.all(deletePromises).then(() => {
        this.selection.clear();
        this.loadData();
        this.showMessage(`${count} documento(s) eliminado(s)`, 'success');
      }).catch(err => {
        console.error('[DocumentoList] Error eliminando multiples:', err);
        this.loadData();
        this.showMessage('Error al eliminar algunos documentos', 'error');
      });
    }
  }

  // ═══════════════════════════════════════════════════════════════════════════
  // EXPORTACION
  // ═══════════════════════════════════════════════════════════════════════════
  exportSelected(): void {
    const data = this.selection.selected;
    if (data.length === 0) return;
    this.exportData(data, 'documentos_seleccionados');
    this.showMessage(`${data.length} registro(s) exportado(s)`, 'success');
  }

  exportAll(): void {
    const data = this.items();
    if (data.length === 0) {
      this.showMessage('No hay datos para exportar', 'error');
      return;
    }
    this.exportData(data, 'documentos_todos');
    this.showMessage('Archivo CSV exportado', 'success');
  }

  private exportData(data: Documento[], filename: string): void {
    const headers = ['Id', 'Nombre', 'Extension', 'MimeType', 'FechaCarga', 'RelacionId', 'RelacionNombre'];
    const rows = data.map(item => [
      item.id?.toString() || '',
      item.nombre || '',
      item.extension || '',
      item.mimeType || '',
      item.fechaCarga?.toString() || '',
      item.relacionId?.toString() || '',
      item.relacionNombre || ''
    ]);
    const csvContent = [headers, ...rows].map(row => row.map(cell => '"' + cell + '"').join(',')).join('\n');
    const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
    const link = document.createElement('a');
    link.href = URL.createObjectURL(blob);
    link.download = filename + '_' + new Date().toISOString().split('T')[0] + '.csv';
    link.click();
  }

  goBack(): void {
    if (this.embedded) return;
    window.history.back();
  }

  toggleFabMenu(): void {
    this.fabMenuExpanded = !this.fabMenuExpanded;
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
