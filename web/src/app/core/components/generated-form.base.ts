import { Directive, OnDestroy, OnInit, inject } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { Observable } from 'rxjs';
import { AsistenteFormBridgeService } from '../services/asistente-form-bridge.service';

// ─── Base de los form components generados (Fase A1.4) ──────────────────────
// Todo el libreto del diálogo vive acá UNA vez: wiring del MatDialog, contexto
// FK del padre, puente del asistente de voz, submit con rechazo del dominio,
// mensajes de error y etiquetas de opciones. El shell por entidad aporta SOLO
// la sustancia: la definición de campos (construirForm), su service y las
// cargas de opciones FK. El HTML sigue generado por entidad (campos concretos).

export interface DatosDialogoForm<T> {
  item?: T;
  mode?: string;
  contextoFk?: { campo: string; valor: any } | null;
}

interface ServicioCrud<T> {
  create(data: any): Observable<T>;
  update(id: number | string, data: any): Observable<T | undefined>;
}

@Directive()
export abstract class GeneratedFormBase<T extends { id: number | string }>
  implements OnInit, OnDestroy {

  protected readonly fb = inject(FormBuilder);
  protected readonly dialogRef = inject(MatDialogRef<unknown>);
  protected readonly puenteVoz = inject(AsistenteFormBridgeService);
  readonly data = inject<DatosDialogoForm<T> | null>(MAT_DIALOG_DATA, { optional: true });

  /** Nombre con el que el form se registra en el puente de voz ('oportunidad'). */
  protected abstract readonly entidad: string;
  protected abstract readonly service: ServicioCrud<T>;
  /** La definición de CAMPOS — la única parte realmente por entidad. */
  protected abstract construirForm(): FormGroup;
  /** Hook para cargar los selects de FK (el shell lo overridea si tiene FKs). */
  protected cargarOpciones(): void {}

  form!: FormGroup;
  isEditing = false;
  isSaving = false;
  item: T | null = null;
  campoBloqueado: string | null = null;   // FK fijado por el contexto del padre (se oculta en el HTML)

  constructor() {
    this.item = this.data?.item || null;
    this.isEditing = !!this.item;
  }

  ngOnInit(): void {
    this.form = this.construirForm();

    // Contexto del padre: al crear un hijo desde una lista filtrada, el FK ya viene dado.
    // El control queda ENABLED (para que viaje en form.value) pero se oculta en el HTML.
    if (!this.isEditing && this.data?.contextoFk) {
      const ctrl = this.form.get(this.data.contextoFk.campo);
      if (ctrl) {
        ctrl.setValue(this.data.contextoFk.valor);
        this.campoBloqueado = this.data.contextoFk.campo;
      }
    }
    this.cargarOpciones();

    // Puente del asistente de voz: este form queda operable por conversación
    // (llenar campos y disparar el submit REAL — validators y dominio deciden)
    this.puenteVoz.registrarForm({
      entidad: this.entidad,
      form: this.form,
      guardar: () => this.onSave(),
      cancelar: () => this.onCancel()
    });
  }

  ngOnDestroy(): void {
    this.puenteVoz.quitarForm(this.entidad);
  }

  isFieldValid(field: string): boolean {
    const control = this.form.get(field);
    return control ? control.valid && control.value !== null && control.value !== '' : false;
  }

  // FK fijado por el contexto del padre: no se muestra (ya está resuelto).
  esFkContexto(campo: string): boolean {
    return this.campoBloqueado === campo;
  }

  onCancel(): void {
    this.dialogRef.close(false);
  }

  onSave(): void {
    if (this.form.invalid) {
      const faltantes: string[] = [];
      Object.keys(this.form.controls).forEach(key => {
        this.form.get(key)?.markAsTouched();
        if (this.form.get(key)?.invalid) { faltantes.push(key); }
      });
      this.puenteVoz.notificarResultado(false, 'campos inválidos o faltantes: ' + faltantes.join(', '));
      return;
    }

    this.isSaving = true;
    const formData = this.form.value;

    const operation = this.isEditing && this.item
      ? this.service.update(this.item.id, { ...formData, id: this.item.id })
      : this.service.create(formData);

    operation.subscribe({
      next: () => {
        this.isSaving = false;
        this.puenteVoz.notificarResultado(true, this.isEditing ? 'cambios guardados' : 'registro creado');
        this.dialogRef.close(true);
      },
      error: (err: any) => {
        console.error('[' + this.entidad + 'Form] Error guardando:', err);
        this.isSaving = false;
        // El rechazo del DOMINIO (reglas de negocio) llega acá: contarlo tal cual.
        // Shapes soportados: RFC 7807 problem+json (Ola 1: detail / errors[].message) y
        // los formatos previos (array [{message}] u objeto {message}).
        const detalle = err?.error?.detail || err?.error?.errors?.[0]?.message ||
          err?.error?.[0]?.message || err?.error?.message ||
          (Array.isArray(err?.error) ? err.error.map((e: any) => e.message).join('; ') : 'el sistema rechazó el guardado');
        this.puenteVoz.notificarResultado(false, detalle);
      }
    });
  }

  getErrorMessage(field: string): string {
    const control = this.form.get(field);
    if (!control) return '';
    if (control.hasError('required')) return 'Este campo es requerido';
    if (control.hasError('email')) return 'Ingresá un email válido (ej: nombre@empresa.com)';
    if (control.hasError('min')) return `El valor mínimo es ${control.getError('min')?.min}`;
    if (control.hasError('max')) return `El valor máximo es ${control.getError('max')?.max}`;
    if (control.hasError('maxlength')) return `Máximo ${control.getError('maxlength')?.requiredLength} caracteres`;
    return 'Valor inválido';
  }

  // Etiqueta legible para opciones de FK: nunca mostrar el id crudo al usuario.
  // Prueba el display configurado y después alternativas comunes; último recurso: #id.
  etiquetaOpcion(option: any, displayProp: string): string {
    for (const c of [displayProp, 'nombre', 'razonSocial', 'descripcion', 'codigo', 'titulo']) {
      const v = option?.[c];
      if (v !== null && v !== undefined && String(v).trim() !== '') return String(v);
    }
    return option?.id != null ? `#${option.id}` : '';
  }
}
