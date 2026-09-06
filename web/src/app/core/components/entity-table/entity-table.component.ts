import { CommonModule } from '@angular/common';
import {
  ChangeDetectionStrategy, Component, ContentChildren, Directive, EventEmitter,
  Input, Output, QueryList, TemplateRef, computed, signal,
} from '@angular/core';
import { SelectionModel } from '@angular/cdk/collections';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatSortModule, Sort } from '@angular/material/sort';
import { MatTableModule } from '@angular/material/table';
import { MatTooltipModule } from '@angular/material/tooltip';
import {
  AccionCustomDescriptor, CampoDescriptor, EntityDescriptor, RelacionHijaDescriptor, columnasDe,
} from '../../models/entity-descriptor';

/**
 * Columna CUSTOM por entidad (la válvula de escape — equivalente frontend de Hooks.cs):
 * el shell proyecta un ng-template para UNA columna y la tabla lo usa en vez del render
 * estándar. El 95% sigue el libreto genérico; lo particular tiene un lugar acotado.
 *
 *   <ceskia-entity-table [descriptor]="D" [items]="items()">
 *     <ng-template ceskiaColumna="monto" let-item let-campo="campo">
 *       <span [class.alerta]="item.monto > 10000">{{ item.monto }}</span>
 *     </ng-template>
 *   </ceskia-entity-table>
 */
@Directive({ selector: 'ng-template[ceskiaColumna]', standalone: true })
export class CeskiaColumnaDirective {
  /** Nombre camelCase de la columna que este template reemplaza. */
  @Input({ required: true, alias: 'ceskiaColumna' }) columna!: string;
  constructor(public readonly template: TemplateRef<unknown>) {}
}

/**
 * Tabla genérica de entidad (A1.5 — multi-vista genérica, vista 1 de 8).
 * Antes: el HTML de la tabla se ESTAMPABA por entidad (un matColumnDef por campo,
 * con los nombres incrustados). Ahora: UN componente que renderiza columnas desde el
 * EntityDescriptor. Paridad con la tabla estampada: selección múltiple, columnas
 * visibles (menú del shell vía columnasVisibles), sort, paginación en memoria,
 * empty states con búsqueda. Usa las MISMAS clases (smart-table, action-btn,
 * col-primary, tnum, …) — los estilos viven en styles/generated/_entity-list.scss.
 */
@Component({
  selector: 'ceskia-entity-table',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    CommonModule, MatTableModule, MatSortModule, MatPaginatorModule, MatCheckboxModule,
    MatIconModule, MatButtonModule, MatMenuModule, MatTooltipModule,
  ],
  template: `
    <div class="table-wrapper">
      <table mat-table [dataSource]="pagina()" matSort (matSortChange)="ordenar($event)" class="smart-table">

        <ng-container matColumnDef="select">
          <th mat-header-cell *matHeaderCellDef>
            <mat-checkbox (change)="toggleTodas()" [checked]="todasSeleccionadas()"
                          [indeterminate]="seleccion.hasValue() && !todasSeleccionadas()"></mat-checkbox>
          </th>
          <td mat-cell *matCellDef="let row">
            <mat-checkbox (click)="$event.stopPropagation()" (change)="toggleFila(row)"
                          [checked]="seleccion.isSelected(row)"></mat-checkbox>
          </td>
        </ng-container>

        @for (col of columnas(); track col.nombre) {
          <ng-container [matColumnDef]="col.nombre">
            <th mat-header-cell *matHeaderCellDef mat-sort-header
                [class.col-num]="esNumerica(col)">{{ col.label }}</th>
            <td mat-cell *matCellDef="let item"
                [class.col-primary]="col.primario"
                [class.tnum]="esNumerica(col)">
              @if (plantillaDe(col.nombre); as tpl) {
                <ng-container [ngTemplateOutlet]="tpl"
                              [ngTemplateOutletContext]="{ $implicit: item, campo: col }" />
              } @else {
                {{ valor(item, col) }}
              }
            </td>
          </ng-container>
        }

        <ng-container matColumnDef="actions">
          <th mat-header-cell *matHeaderCellDef></th>
          <td mat-cell *matCellDef="let item">
            <div class="action-buttons" (click)="$event.stopPropagation()">
              <button mat-icon-button class="action-btn" matTooltip="Ver ficha"
                      (click)="ver.emit(item)"><mat-icon>visibility</mat-icon></button>
              <button mat-icon-button class="action-btn edit" matTooltip="Editar"
                      (click)="editar.emit(item)"><mat-icon>edit</mat-icon></button>
              <button mat-icon-button class="action-btn delete" matTooltip="Eliminar"
                      (click)="borrar.emit(item)"><mat-icon>delete_outline</mat-icon></button>
              <button mat-icon-button class="action-btn" [matMenuTriggerFor]="menuFila"
                      matTooltip="Más acciones"><mat-icon>more_horiz</mat-icon></button>
              <mat-menu #menuFila="matMenu">
                @for (h of descriptor.hijas ?? []; track h.entidad) {
                  <button mat-menu-item (click)="verHija.emit({ hija: h, item })">
                    <mat-icon>{{ h.icon }}</mat-icon><span>Ver {{ h.entidad }}</span>
                  </button>
                }
                @for (a of descriptor.acciones ?? []; track a.key) {
                  <button mat-menu-item (click)="accion.emit({ accion: a, item })">
                    <mat-icon>{{ a.icon }}</mat-icon><span>{{ a.label }}</span>
                  </button>
                }
                <button mat-menu-item (click)="documentos.emit(item)">
                  <mat-icon>folder</mat-icon><span>Documentos</span>
                </button>
              </mat-menu>
            </div>
          </td>
        </ng-container>

        <tr mat-header-row *matHeaderRowDef="nombresColumnas()"></tr>
        <tr mat-row *matRowDef="let row; columns: nombresColumnas();"
            [class.selected]="seleccion.isSelected(row)" (click)="toggleFila(row)"></tr>

        <tr class="mat-row" *matNoDataRow>
          @if (!cargando) {
            <td class="mat-cell empty-state-cell" [attr.colspan]="nombresColumnas().length">
              <div class="empty-state">
                @if (termino) {
                  <mat-icon>search_off</mat-icon>
                  <h3>Sin resultados para «{{ termino }}»</h3>
                  <button mat-stroked-button class="empty-btn" (click)="limpiarBusqueda.emit()">
                    <mat-icon>close</mat-icon>Limpiar búsqueda</button>
                } @else {
                  <mat-icon>inventory_2</mat-icon>
                  <h3>Todavía no hay registros</h3>
                }
              </div>
            </td>
          }
        </tr>
      </table>
    </div>
    <mat-paginator [length]="ordenados().length" [pageSizeOptions]="[25, 50, 100]" [pageSize]="tamPagina()"
                   showFirstLastButtons aria-label="Paginación" (page)="paginar($event)"></mat-paginator>
  `,
})
export class EntityTableComponent {
  @Input({ required: true }) descriptor!: EntityDescriptor;
  @Input() set items(value: any[]) { this._items.set(value ?? []); }
  @Input() cargando = false;
  /** Término de búsqueda activo (solo para el texto del empty state). */
  @Input() termino = '';
  /** Columnas ocultas por el usuario (el menú de columnas vive en el shell/toolbar). */
  @Input() set columnasOcultas(value: string[]) { this._ocultas.set(value ?? []); }

  @Output() ver = new EventEmitter<any>();
  @Output() editar = new EventEmitter<any>();
  @Output() borrar = new EventEmitter<any>();
  @Output() crear = new EventEmitter<void>();
  @Output() limpiarBusqueda = new EventEmitter<void>();
  @Output() accion = new EventEmitter<{ accion: AccionCustomDescriptor; item: any }>();
  @Output() verHija = new EventEmitter<{ hija: RelacionHijaDescriptor; item: any }>();
  @Output() documentos = new EventEmitter<any>();
  @Output() seleccionCambio = new EventEmitter<any[]>();

  /** Templates de columna proyectados por el shell (ng-template[ceskiaColumna]). */
  @ContentChildren(CeskiaColumnaDirective) plantillas?: QueryList<CeskiaColumnaDirective>;

  readonly seleccion = new SelectionModel<any>(true, []);

  private readonly _items = signal<any[]>([]);
  private readonly _ocultas = signal<string[]>([]);
  private readonly _orden = signal<Sort | null>(null);
  private readonly _pagina = signal(0);
  readonly tamPagina = signal(25);

  readonly columnas = computed(() =>
    columnasDe(this.descriptor).filter(c => !this._ocultas().includes(c.nombre)));
  readonly nombresColumnas = computed(() =>
    ['select', ...this.columnas().map(c => c.nombre), 'actions']);

  readonly ordenados = computed(() => {
    const items = this._items();
    const orden = this._orden();
    if (!orden?.active || !orden.direction) return items;
    const col = columnasDe(this.descriptor).find(c => c.nombre === orden.active);
    const dir = orden.direction === 'asc' ? 1 : -1;
    return [...items].sort((a, b) => {
      const va = col ? this.crudo(a, col) : a[orden.active];
      const vb = col ? this.crudo(b, col) : b[orden.active];
      if (va == null) return 1;
      if (vb == null) return -1;
      if (typeof va === 'number' && typeof vb === 'number') return (va - vb) * dir;
      return String(va).localeCompare(String(vb), 'es') * dir;
    });
  });

  readonly pagina = computed(() => {
    const desde = this._pagina() * this.tamPagina();
    return this.ordenados().slice(desde, desde + this.tamPagina());
  });

  plantillaDe(columna: string): TemplateRef<unknown> | null {
    return this.plantillas?.find(p => p.columna === columna)?.template ?? null;
  }

  ordenar(orden: Sort): void { this._orden.set(orden); this._pagina.set(0); }

  paginar(ev: PageEvent): void {
    this._pagina.set(ev.pageIndex);
    this.tamPagina.set(ev.pageSize);
  }

  toggleFila(row: any): void {
    this.seleccion.toggle(row);
    this.seleccionCambio.emit(this.seleccion.selected);
  }

  toggleTodas(): void {
    if (this.todasSeleccionadas()) this.seleccion.clear();
    else this.seleccion.select(...this.pagina());
    this.seleccionCambio.emit(this.seleccion.selected);
  }

  todasSeleccionadas(): boolean {
    const pag = this.pagina();
    return pag.length > 0 && pag.every(i => this.seleccion.isSelected(i));
  }

  esNumerica(col: CampoDescriptor): boolean {
    return col.tipo === 'numero' || col.tipo === 'moneda';
  }

  /** Valor CRUDO para ordenar (fk ordena por display). */
  private crudo(item: any, col: CampoDescriptor): any {
    if (col.tipo === 'fk' && col.display) return item[col.display] ?? item[col.nombre];
    return item[col.nombre];
  }

  /** Valor FORMATEADO por tipo funcional — antes esto era interpolación estampada. */
  valor(item: any, col: CampoDescriptor): string {
    const v = this.crudo(item, col);
    if (v == null || v === '') return '—';
    switch (col.tipo) {
      case 'moneda':
        return new Intl.NumberFormat('es-UY', { style: 'currency', currency: 'UYU', currencyDisplay: 'narrowSymbol' }).format(Number(v));
      case 'numero':
        return new Intl.NumberFormat('es-UY').format(Number(v));
      case 'fecha': {
        const fecha = new Date(v);
        return isNaN(fecha.getTime())
          ? String(v)
          : new Intl.DateTimeFormat('es-UY').format(fecha);
      }
      case 'bool':
        return v ? 'Sí' : 'No';
      default:
        return String(v);
    }
  }
}
