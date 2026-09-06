import { Injectable } from '@angular/core';
import { FormGroup } from '@angular/forms';

export interface FormRegistrado {
  entidad: string;
  form: FormGroup;
  guardar: () => void;
  cancelar: () => void;
}

export interface ResultadoGuardado { ok: boolean; detalle: string; }

/**
 * PUENTE entre el asistente de voz y las pantallas generadas. Las listas registran
 * cómo abrir su alta, las fichas cómo editar su registro, y los formularios se
 * registran al abrirse. El asistente NUNCA toca datos: llena controles del form
 * real (visible, con destello) y el guardado pasa por el submit normal — validators
 * del form + reglas del dominio deciden. BORRAR no existe en este puente, a propósito.
 */
@Injectable({ providedIn: 'root' })
export class AsistenteFormBridgeService {
  private readonly abridores = new Map<string, () => void>();
  private readonly editores = new Map<string, () => void>();
  private formActual: FormRegistrado | null = null;
  private resolverGuardado: ((r: ResultadoGuardado) => void) | null = null;

  // ── Registro desde las pantallas generadas ──
  registrarAbridor(entidad: string, abrir: () => void): void { this.abridores.set(entidad, abrir); }
  quitarAbridor(entidad: string): void { this.abridores.delete(entidad); }

  registrarEditor(entidad: string, editar: () => void): void { this.editores.set(entidad, editar); }
  quitarEditor(entidad: string): void { this.editores.delete(entidad); }

  registrarForm(reg: FormRegistrado): void { this.formActual = reg; }

  quitarForm(entidad: string): void {
    if (this.formActual?.entidad === entidad) {
      this.formActual = null;
      // Si había un guardado esperando y el form se cerró sin resultado, avisar honesto
      this.resolverGuardado?.({ ok: false, detalle: 'el formulario se cerró' });
      this.resolverGuardado = null;
    }
  }

  /** Lo llama el form generado al terminar su onSave (éxito o rechazo). */
  notificarResultado(ok: boolean, detalle: string): void {
    this.resolverGuardado?.({ ok, detalle });
    this.resolverGuardado = null;
  }

  // ── Manos del asistente ──
  hayForm(): boolean { return this.formActual !== null; }
  entidadDelForm(): string | null { return this.formActual?.entidad ?? null; }

  abrirAlta(entidad: string): boolean {
    const abrir = this.abridores.get(entidad);
    if (!abrir) { return false; }
    abrir();
    return true;
  }

  editarActual(entidad: string): boolean {
    const editar = this.editores.get(entidad);
    if (!editar) { return false; }
    editar();
    return true;
  }

  llenarCampo(campo: string, valor: unknown): { ok: boolean; detalle: string } {
    const f = this.formActual;
    if (!f) { return { ok: false, detalle: 'no hay ningún formulario abierto' }; }
    const control = f.form.get(campo);
    if (!control) { return { ok: false, detalle: `el campo "${campo}" no existe en este formulario` }; }

    control.patchValue(valor);
    control.markAsDirty();
    control.markAsTouched();
    this.destellar(campo);

    if (control.invalid) {
      return { ok: false, detalle: `"${campo}" quedó inválido: ${this.motivoInvalido(control)}` };
    }
    return { ok: true, detalle: `${campo} = ${String(valor)}` };
  }

  /** Estado del form para que el asistente sepa qué falta (nunca adivina). */
  estadoForm(): { campo: string; valor: unknown; valido: boolean }[] {
    const f = this.formActual;
    if (!f) { return []; }
    return Object.keys(f.form.controls).map(campo => {
      const c = f.form.get(campo)!;
      return { campo, valor: c.value, valido: c.valid };
    });
  }

  /** Dispara el submit REAL del form y espera su veredicto (validators + dominio). */
  guardar(): Promise<ResultadoGuardado> {
    const f = this.formActual;
    if (!f) { return Promise.resolve({ ok: false, detalle: 'no hay ningún formulario abierto' }); }
    return new Promise<ResultadoGuardado>((resolve) => {
      this.resolverGuardado = resolve;
      f.guardar();
      // Red de seguridad: si nada respondió en 15s, no dejar la voz colgada
      setTimeout(() => { this.resolverGuardado?.({ ok: false, detalle: 'el guardado no respondió' }); this.resolverGuardado = null; }, 15000);
    });
  }

  cancelar(): void { this.formActual?.cancelar(); }

  // ── Teatro visual: el campo destella mientras la conversación se vuelve datos ──
  private destellar(campo: string): void {
    setTimeout(() => {
      const el = document.querySelector(`[formcontrolname="${campo}"]`)?.closest('.form-field-wrapper, .form-toggle-wrapper');
      if (!el) { return; }
      el.classList.add('av-destello');
      setTimeout(() => el.classList.remove('av-destello'), 1400);
    }, 30);
  }

  private motivoInvalido(control: import('@angular/forms').AbstractControl): string {
    if (control.hasError('required')) { return 'es requerido y quedó vacío'; }
    if (control.hasError('min')) { return 'está por debajo del mínimo'; }
    if (control.hasError('max')) { return 'supera el máximo'; }
    return 'no pasa la validación';
  }
}
