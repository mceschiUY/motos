import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MarcaService } from '../../generated/services/marca.service';
import { CategoriaService } from '../../generated/services/categoria.service';
import { Marca } from '../../generated/models/marca.model';
import { Categoria } from '../../generated/models/categoria.model';
import { CatalogoService } from './catalogo.service';
import { CatalogoItem } from './catalogo.model';

/** Card + su portada ya resuelta a data-URL (o null mientras viaja / si no tiene). */
interface Card { item: CatalogoItem; foto: string | null; }

/**
 * Catálogo comercial — pantalla artesanal de SOLO LECTURA (plan §4.5, Etapa C).
 * Es la pantalla que se le muestra al cliente en la tienda: grilla de productos con
 * foto, precio "desde" en USD y stock, destacados primero y etiqueta "Nuevo".
 *
 * Las fotos se piden de a una en base64 (`/api/Documentos/{id}`): `download/{id}` está
 * bajo [Authorize] y un `<img src>` no lleva el JWT que pone el interceptor. Con el
 * catálogo de la demo (10 productos) son 10 pedidos livianos, cacheados en el service.
 */
@Component({
  selector: 'app-catalogo',
  standalone: true,
  imports: [
    CommonModule, FormsModule,
    MatIconModule, MatButtonModule, MatTooltipModule,
    MatFormFieldModule, MatSelectModule, MatProgressSpinnerModule,
  ],
  templateUrl: './catalogo.component.html',
  styleUrl: './catalogo.component.scss',
})
export class CatalogoComponent implements OnInit {
  private readonly service = inject(CatalogoService);
  private readonly marcaService = inject(MarcaService);
  private readonly categoriaService = inject(CategoriaService);
  private readonly router = inject(Router);

  readonly isLoading = signal(false);
  readonly cards = signal<Card[]>([]);
  readonly marcas = signal<Marca[]>([]);
  readonly categorias = signal<Categoria[]>([]);

  // Filtros server-side: el endpoint acepta marcaId / categoriaId / q.
  readonly marcaId = signal<number | null>(null);
  readonly categoriaId = signal<number | null>(null);
  readonly busqueda = signal('');

  readonly destacados = computed(() => this.cards().filter(c => c.item.destacado).length);
  readonly novedades = computed(() => this.cards().filter(c => c.item.novedad).length);
  readonly conFoto = computed(() => this.cards().filter(c => c.item.imagenPrincipalId != null).length);

  ngOnInit(): void {
    this.marcaService.getAll().subscribe({
      next: (d: Marca[]) => this.marcas.set(d.filter(m => m.activo !== false)),
      error: (e: unknown) => console.error('[Catalogo] marcas:', e),
    });
    this.categoriaService.getAll().subscribe({
      next: (d: Categoria[]) => this.categorias.set(d.filter(c => c.activo !== false)),
      error: (e: unknown) => console.error('[Catalogo] categorías:', e),
    });
    this.cargar();
  }

  cargar(): void {
    this.isLoading.set(true);
    this.service.catalogo(this.marcaId(), this.categoriaId(), this.busqueda().trim() || null).subscribe({
      next: (items: CatalogoItem[]) => {
        this.cards.set((items || []).map(item => ({ item, foto: null })));
        this.isLoading.set(false);
        this.cargarFotos();
      },
      error: (e: unknown) => { console.error('[Catalogo] catálogo:', e); this.cards.set([]); this.isLoading.set(false); },
    });
  }

  /** Cada portada se pide por separado y se va pintando; el orden de llegada no importa. */
  private cargarFotos(): void {
    for (const card of this.cards()) {
      const docId = card.item.imagenPrincipalId;
      if (docId == null) { continue; }
      this.service.portada(docId).subscribe(src => {
        this.cards.update(cs => cs.map(c => c.item.id === card.item.id ? { ...c, foto: src } : c));
      });
    }
  }

  limpiarFiltros(): void {
    this.marcaId.set(null);
    this.categoriaId.set(null);
    this.busqueda.set('');
    this.cargar();
  }

  hayFiltros(): boolean {
    return this.marcaId() != null || this.categoriaId() != null || this.busqueda().trim().length > 0;
  }

  abrir(item: CatalogoItem): void { this.router.navigate(['/catalogo', item.id]); }

  /** Iniciales de la marca, para la card sin foto: mejor que un ícono genérico. */
  iniciales(item: CatalogoItem): string {
    const texto = item.marcaDisplay || item.nombre || '?';
    return texto.split(/\s+/).slice(0, 2).map(p => p.charAt(0).toUpperCase()).join('');
  }
}
