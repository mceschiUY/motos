import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { SiteConfigService } from '../../../../core/services/site-config.service';
import {
  ConfiguracionItem,
  CONFIG_GROUPS,
  UpdateConfigBatchItem
} from '../../../../core/models/site-config.model';

interface ConfigGroup {
  key: string;
  label: string;
  icon: string;
  items: ConfiguracionItem[];
}

@Component({
  selector: 'app-configuracion-sitio',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatIconModule,
    MatButtonModule,
    MatInputModule,
    MatFormFieldModule,
    MatCardModule,
    MatProgressSpinnerModule,
    MatSnackBarModule
  ],
  templateUrl: './configuracion-sitio.component.html',
  styleUrl: './configuracion-sitio.component.scss'
})
export class ConfiguracionSitioComponent implements OnInit {
  private siteConfigService = inject(SiteConfigService);
  private snackBar = inject(MatSnackBar);

  // Estado
  readonly isLoading = signal(true);
  readonly isSaving = signal(false);
  readonly groups = signal<ConfigGroup[]>([]);
  readonly hasChanges = signal(false);

  // Valores originales para detectar cambios
  private originalValues = new Map<number, string>();

  // Mapeo de labels amigables
  private readonly labelMap: Record<string, string> = {
    'sitio.nombre': 'Nombre del Sitio',
    'sitio.subtitulo': 'Subtitulo',
    'sitio.logo_url': 'URL del Logo',
    'sitio.favicon_url': 'URL del Favicon',
    'empresa.nombre': 'Nombre de la Empresa',
    'empresa.email': 'Email de Contacto',
    'empresa.telefono': 'Telefono',
    'empresa.direccion': 'Direccion',
    'empresa.horario': 'Horario de Atencion',
    'social.facebook': 'Facebook',
    'social.instagram': 'Instagram',
    'social.linkedin': 'LinkedIn',
    'social.twitter': 'Twitter / X',
    'social.whatsapp': 'WhatsApp',
    'social.youtube': 'YouTube',
    'legal.copyright': 'Texto de Copyright'
  };

  ngOnInit(): void {
    this.loadConfig();
  }

  loadConfig(): void {
    this.isLoading.set(true);
    this.siteConfigService.getAllItems().subscribe({
      next: (items) => {
        this.organizeByGroups(items);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('[ConfigAdmin] Error cargando config:', err);
        this.isLoading.set(false);
        this.showMessage('Error al cargar la configuracion', true);
      }
    });
  }

  private organizeByGroups(items: ConfiguracionItem[]): void {
    const grouped: ConfigGroup[] = CONFIG_GROUPS.map(g => ({
      ...g,
      items: items
        .filter(item => item.grupo === g.key)
        .sort((a, b) => a.orden - b.orden)
    }));

    // Guardar valores originales
    this.originalValues.clear();
    items.forEach(item => this.originalValues.set(item.id, item.valor));

    this.groups.set(grouped);
    this.hasChanges.set(false);
  }

  getLabel(clave: string): string {
    return this.labelMap[clave] || clave;
  }

  getInputType(tipo: string): string {
    switch (tipo) {
      case 'email': return 'email';
      case 'url': return 'url';
      default: return 'text';
    }
  }

  onValueChange(item: ConfiguracionItem): void {
    const original = this.originalValues.get(item.id);
    const allItems = this.groups().flatMap(g => g.items);
    const hasAnyChange = allItems.some(i =>
      this.originalValues.get(i.id) !== i.valor
    );
    this.hasChanges.set(hasAnyChange);
  }

  save(): void {
    if (!this.hasChanges()) return;

    this.isSaving.set(true);

    // Recopilar solo los items modificados
    const allItems = this.groups().flatMap(g => g.items);
    const changes: UpdateConfigBatchItem[] = allItems
      .filter(item => this.originalValues.get(item.id) !== item.valor)
      .map(item => ({ id: item.id, valor: item.valor }));

    if (changes.length === 0) {
      this.isSaving.set(false);
      return;
    }

    this.siteConfigService.updateBatch(changes).subscribe({
      next: () => {
        // Actualizar valores originales
        changes.forEach(c => {
          const item = allItems.find(i => i.id === c.id);
          if (item) this.originalValues.set(item.id, item.valor);
        });
        this.hasChanges.set(false);
        this.isSaving.set(false);
        this.showMessage('Configuracion guardada correctamente');
      },
      error: (err) => {
        console.error('[ConfigAdmin] Error guardando:', err);
        this.isSaving.set(false);
        this.showMessage('Error al guardar la configuracion', true);
      }
    });
  }

  reset(): void {
    // Restaurar valores originales
    const groups = this.groups();
    groups.forEach(group => {
      group.items.forEach(item => {
        const original = this.originalValues.get(item.id);
        if (original !== undefined) {
          item.valor = original;
        }
      });
    });
    this.groups.set([...groups]);
    this.hasChanges.set(false);
  }

  private showMessage(message: string, isError = false): void {
    this.snackBar.open(message, 'Cerrar', {
      duration: 3000,
      horizontalPosition: 'end',
      verticalPosition: 'top',
      panelClass: isError ? ['error-snackbar'] : ['success-snackbar']
    });
  }
}
