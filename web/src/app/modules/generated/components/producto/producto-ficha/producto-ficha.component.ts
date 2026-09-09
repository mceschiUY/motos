import { Component, OnInit, OnDestroy, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { environment } from '../../../../../../environments/environment';
import { AsistenteFormBridgeService } from '../../../../../core/services/asistente-form-bridge.service';

import { Producto } from '../../../models/producto.model';
import { ProductoService } from '../../../services/producto.service';
import { ProductoFormComponent } from '../producto-form/producto-form.component';
import { FotoGaleriaComponent } from '../../../../../shared/components/foto-galeria/foto-galeria.component';
import { MembreteImpresionComponent } from '../../../../../shared/components/membrete-impresion/membrete-impresion.component';

interface EventoHistoria { fecha: string; quien: string; que: string; ok: boolean; }
interface RelacionFicha { nombre: string; label: string; icon: string; ruta: string; endpoint: string; }

@Component({
  selector: 'app-producto-ficha',
  standalone: true,
  imports: [CommonModule, RouterModule, MatIconModule, MatButtonModule, MatTooltipModule, MatDialogModule, MatSnackBarModule, FotoGaleriaComponent, MembreteImpresionComponent],
  templateUrl: './producto-ficha.component.html',
  styleUrl: './producto-ficha.component.scss'
})
export class ProductoFichaComponent implements OnInit, OnDestroy {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly http = inject(HttpClient);
  private readonly service = inject(ProductoService);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);
  private readonly puenteVoz = inject(AsistenteFormBridgeService);

  readonly item = signal<Producto | null>(null);
  readonly cargando = signal<boolean>(true);
  readonly historia = signal<EventoHistoria[]>([]);
  readonly relacionados = signal<Record<string, any[]>>({});
  readonly tabActiva = signal<string>('');
  id: string | number = '';

  readonly relaciones: RelacionFicha[] = [
    { nombre: 'variante', label: 'Variantes', icon: 'qr_code_2', ruta: '/variante', endpoint: '/Variante/by-producto/' }
  ];

  ngOnInit(): void {
    this.route.paramMap.subscribe(pm => {
      this.id = pm.get('id') ?? '';
      if (this.relaciones.length > 0) { this.tabActiva.set(this.relaciones[0].nombre); }
      this.cargar();
    });
    // Puente del asistente de voz: "editá este registro" abre el form de ESTA ficha
    this.puenteVoz.registrarEditor('producto', () => this.editar());
  }

  ngOnDestroy(): void {
    this.puenteVoz.quitarEditor('producto');
  }

  cargar(): void {
    this.cargando.set(true);
    this.service.getById(String(this.id)).subscribe({
      next: (data) => { this.item.set(data ?? null); this.cargando.set(false); },
      error: () => { this.item.set(null); this.cargando.set(false); }
    });
    this.cargarHistoria();
    for (const r of this.relaciones) { this.cargarRelacion(r); }
  }

  titulo(): string {
    const i = this.item() as any;
    return i ? (i.nombre || ('Producto #' + i.id)) : '';
  }

  volver(): void { this.router.navigate(['/producto']); }

  editar(): void {
    const ref = this.dialog.open(ProductoFormComponent, {
      width: '600px', maxWidth: '95vw', panelClass: 'crm-dialog',
      data: { item: this.item(), mode: 'edit' }
    });
    ref.afterClosed().subscribe((ok: unknown) => { if (ok) { this.cargar(); } });
  }

  async eliminar(): Promise<void> {
    if (!(await (window as any).confirmar('¿Eliminar este registro?\n\nEsta acción no se puede deshacer.'))) { return; }
    this.service.delete(String(this.id)).subscribe({ next: () => this.volver() });
  }

  documentos(): void {
    this.router.navigate(['/documento'], { queryParams: { relacionId: this.id, relacionNombre: 'Producto' } });
  }

  // PDF con marca: el navegador imprime; el membrete y el @media print hacen el resto
  imprimir(): void { window.print(); }

  // ═══ Historia: la auditoría que el sistema YA registra, por fin visible ═══
  cargarHistoria(): void {
    this.http.get<any[]>(`${environment.apiUrl}/Auditoria/entidad/Producto/${this.id}`).subscribe({
      next: (logs) => this.historia.set((logs || []).map(l => ({
        fecha: l.timestamp, quien: l.userName || 'sistema',
        que: this.traducirAccion(l.action), ok: l.success !== false
      }))),
      error: () => this.historia.set([])
    });
  }

  private traducirAccion(a: string): string {
    const s = a || '';
    const paso = s.match(/PasarA([A-Za-z]+)/);
    if (paso) { return 'Pasó a ' + paso[1].replace(/([A-Z])/g, ' $1').trim().toLowerCase(); }
    if (s.includes('Crear')) { return 'Creado'; }
    if (s.includes('Modificar')) { return 'Modificado'; }
    if (s.includes('Eliminar')) { return 'Eliminado'; }
    return s;
  }

  // ═══ Relacionados ═══
  cargarRelacion(r: RelacionFicha): void {
    this.http.get<any[]>(`${environment.apiUrl}${r.endpoint}${this.id}`).subscribe({
      next: (items) => this.relacionados.update(m => ({ ...m, [r.nombre]: items || [] })),
      error: () => this.relacionados.update(m => ({ ...m, [r.nombre]: [] }))
    });
  }

  itemsDe(nombre: string): any[] { return this.relacionados()[nombre] || []; }

  columnasDe(nombre: string): string[] {
    const items = this.itemsDe(nombre);
    if (items.length === 0) { return []; }
    return Object.keys(items[0]).filter(k => k !== 'id' && !k.toLowerCase().endsWith('id')).slice(0, 4);
  }

  irA(r: RelacionFicha, row: any): void { this.router.navigate([r.ruta, row.id]); }

  verEnLista(r: RelacionFicha): void { this.router.navigate([r.ruta]); }
}
