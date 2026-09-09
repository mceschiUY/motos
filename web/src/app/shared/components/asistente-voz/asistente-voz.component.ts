import { Component, OnDestroy, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { environment } from '../../../../environments/environment';
import { BUSQUEDA_ENTIDADES } from '../../../modules/generated/generated-search.registry';
import { SiteConfigService } from '../../../core/services/site-config.service';
import { AsistenteFormBridgeService } from '../../../core/services/asistente-form-bridge.service';

interface SesionVoz { clientSecret: string; model: string; voice: string; }
type EstadoVoz = 'inactivo' | 'conectando' | 'escuchando' | 'pensando' | 'hablando' | 'error';

/** Secciones FIJAS del core (iguales en todos los productos): el asistente también
 *  navega el sistema base, no solo las entidades del negocio. */
const SECCIONES_CORE: { clave: string; ruta: string; label: string }[] = [
  { clave: 'usuarios', ruta: '/seguridad/usuarios', label: 'Usuarios (seguridad)' },
  { clave: 'perfiles', ruta: '/seguridad/perfiles', label: 'Perfiles (seguridad)' },
  { clave: 'roles', ruta: '/seguridad/roles', label: 'Roles (seguridad)' },
  { clave: 'capabilities', ruta: '/seguridad/capabilities', label: 'Capabilities (seguridad)' },
  { clave: 'configuracion', ruta: '/configuracion', label: 'Configuración del sistema' },
  { clave: 'auditoria', ruta: '/auditoria', label: 'Auditoría (quién hizo qué)' },
  { clave: 'reportes', ruta: '/reportes', label: 'Reportes y exportaciones' },
  { clave: 'documentos', ruta: '/documento', label: 'Documentos adjuntos' }
];

/**
 * ASISTENTE DE VOZ (POC: conversar + navegar). Capa OPCIONAL — el botón solo aparece si
 * el backend tiene key configurada. El navegador habla con OpenAI Realtime por WebRTC
 * usando un token EFÍMERO; las únicas manos del asistente son estas herramientas de
 * NAVEGACIÓN (solo lectura): navegar, buscar, abrir ficha, inicio, modo pantalla.
 * Qué entidades existen se lo dice el manifiesto generado — cero config por producto.
 */
@Component({
  selector: 'app-asistente-voz',
  standalone: true,
  imports: [CommonModule, MatIconModule, MatTooltipModule],
  template: `
    @if (habilitado()) {
      <div class="av-root">
        <button class="av-fab" [class.activo]="estado() !== 'inactivo'"
                (click)="estado() === 'inactivo' ? iniciar() : cortar()"
                [matTooltip]="estado() === 'inactivo' ? 'Hablar con el sistema' : 'Cortar'"
                matTooltipPosition="below">
          <mat-icon>{{ estado() === 'inactivo' ? 'mic' : 'mic_off' }}</mat-icon>
          <span class="av-label">{{ estado() === 'inactivo' ? 'Hablar' : etiquetaEstado() }}</span>
        </button>
        @if (estado() !== 'inactivo') {
          <div class="av-panel">
            <div class="av-estado">
              <span class="av-dot" [class]="'av-' + estado()"></span>
              <span class="av-estado-texto">{{ etiquetaEstado() }}</span>
              <button class="av-cerrar" (click)="cortar()" matTooltip="Cortar"><mat-icon>close</mat-icon></button>
            </div>
            @if (ultimaAccion()) {
              <div class="av-accion">{{ ultimaAccion() }}</div>
            }
            @if (error()) {
              <div class="av-error">{{ error() }}</div>
            }
          </div>
        }
      </div>
    }
  `,
  styles: [`
    /* En el cabezal (pedido Pablo 2026-09-06): botón de chrome, ya no FAB flotante.
       El panel de estado se despliega debajo, anclado al botón. */
    .av-root { position: relative; display: flex; align-items: center; }
    .av-fab { display: flex; align-items: center; gap: 6px; height: 32px; padding: 0 12px 0 8px; border-radius: var(--ceskia-radius-full); border: 1px solid color-mix(in srgb, var(--ceskia-accent-primary) 40%, transparent); background: var(--ceskia-surface); color: var(--ceskia-accent-primary); cursor: pointer; font-family: inherit; font-size: var(--ceskia-text-sm); font-weight: var(--ceskia-font-medium); transition: var(--ceskia-transition-fast); }
    .av-fab mat-icon { font-size: 18px; width: 18px; height: 18px; }
    .av-fab:hover { background: var(--ceskia-accent-primary-glow); border-color: var(--ceskia-accent-primary); }
    .av-fab.activo { background: var(--ceskia-accent-primary); border-color: var(--ceskia-accent-primary); color: #ffffff; }
    .av-panel { position: absolute; top: calc(100% + 10px); left: 0; z-index: 1001; width: 300px; background: var(--ceskia-elevated); border: 1px solid var(--ceskia-border-default); border-radius: var(--ceskia-radius-lg); padding: 12px 14px; box-shadow: var(--ceskia-shadow-lg); }
    @media (max-width: 768px) { .av-label { display: none; } .av-fab { padding: 0 8px; } }
    .av-estado { display: flex; align-items: center; gap: 8px; }
    .av-dot { width: 10px; height: 10px; border-radius: 50%; }
    .av-conectando { background: var(--ceskia-accent-warning); animation: av-pulso 1s infinite; }
    .av-escuchando { background: var(--ceskia-accent-success); animation: av-pulso 1.4s infinite; }
    .av-pensando { background: var(--ceskia-accent-info); animation: av-pulso .7s infinite; }
    .av-hablando { background: var(--ceskia-accent-primary); animation: av-pulso 1s infinite; }
    .av-error { color: var(--ceskia-accent-danger); font-size: var(--ceskia-text-xs); margin-top: 6px; }
    @keyframes av-pulso { 0%,100% { opacity: 1; } 50% { opacity: .35; } }
    .av-estado-texto { font-size: var(--ceskia-text-sm); flex: 1; }
    .av-cerrar { background: transparent; border: none; color: var(--ceskia-text-muted); cursor: pointer; display: flex; mat-icon { font-size: 17px; width: 17px; height: 17px; } }
    .av-accion { margin-top: 6px; font-size: var(--ceskia-text-xs); color: var(--ceskia-text-tertiary); }
  `]
})
export class AsistenteVozComponent implements OnInit, OnDestroy {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);
  private readonly siteConfig = inject(SiteConfigService);
  private readonly puente = inject(AsistenteFormBridgeService);

  readonly habilitado = signal(false);
  readonly estado = signal<EstadoVoz>('inactivo');
  readonly ultimaAccion = signal('');
  readonly error = signal('');

  private pc: RTCPeerConnection | null = null;
  private dc: RTCDataChannel | null = null;
  private mic: MediaStream | null = null;
  private audioEl: HTMLAudioElement | null = null;

  // Protocolo Realtime: UNA respuesta activa por vez. response.create se encola si
  // hay una en curso ("Conversation already has an active response in progress").
  private respuestaActiva = false;
  private respuestaPendiente = false;

  // Caché de sesión para catálogos de referencia (zonas, propietarios…): se traen UNA
  // vez y las siguientes consultas son instantáneas — sin ida y vuelta HTTP por campo.
  private readonly cacheRef = new Map<string, { id: number | string; titulo: string }[]>();

  ngOnInit(): void {
    this.http.get<{ habilitado: boolean }>(`${environment.apiUrl}/Asistente/estado`).subscribe({
      next: (r) => this.habilitado.set(!!r?.habilitado),
      error: () => this.habilitado.set(false)
    });
  }

  ngOnDestroy(): void { this.cortar(); }

  etiquetaEstado(): string {
    switch (this.estado()) {
      case 'conectando': return 'Conectando…';
      case 'escuchando': return 'Te escucho';
      case 'pensando': return 'Pensando…';
      case 'hablando': return 'Hablando';
      case 'error': return 'Error';
      default: return '';
    }
  }

  async iniciar(): Promise<void> {
    this.error.set('');
    this.ultimaAccion.set('');
    this.estado.set('conectando');
    try {
      // 1. Token efímero del backend (la key maestra nunca llega acá)
      const sesion = await new Promise<SesionVoz>((res, rej) =>
        this.http.post<SesionVoz>(`${environment.apiUrl}/Asistente/sesion`, {}).subscribe({ next: res, error: rej }));

      // 2. Micrófono + conexión WebRTC con OpenAI
      this.mic = await navigator.mediaDevices.getUserMedia({ audio: true });
      this.pc = new RTCPeerConnection();
      this.mic.getTracks().forEach(t => this.pc!.addTrack(t, this.mic!));

      this.audioEl = new Audio();
      this.audioEl.autoplay = true;
      this.pc.ontrack = (e) => { if (this.audioEl) { this.audioEl.srcObject = e.streams[0]; } };

      this.dc = this.pc.createDataChannel('oai-events');
      this.dc.onopen = () => this.configurarSesion();
      this.dc.onmessage = (e) => this.onEvento(e.data);

      const offer = await this.pc.createOffer();
      await this.pc.setLocalDescription(offer);

      const resp = await fetch(`https://api.openai.com/v1/realtime/calls?model=${encodeURIComponent(sesion.model)}`, {
        method: 'POST',
        headers: { 'Authorization': `Bearer ${sesion.clientSecret}`, 'Content-Type': 'application/sdp' },
        body: offer.sdp
      });
      if (!resp.ok) { throw new Error('El proveedor de voz rechazó la llamada (' + resp.status + ')'); }
      await this.pc.setRemoteDescription({ type: 'answer', sdp: await resp.text() });

      this.estado.set('escuchando');
    } catch (e: any) {
      console.error('[AsistenteVoz] no se pudo iniciar:', e);
      this.error.set(e?.error?.message || e?.message || 'No se pudo iniciar la sesión de voz');
      this.estado.set('error');
      this.liberar();
    }
  }

  cortar(): void {
    this.liberar();
    this.estado.set('inactivo');
  }

  private liberar(): void {
    this.dc?.close(); this.dc = null;
    this.pc?.close(); this.pc = null;
    this.mic?.getTracks().forEach(t => t.stop()); this.mic = null;
    if (this.audioEl) { this.audioEl.srcObject = null; this.audioEl = null; }
  }

  // ═══════════ Sesión: instrucciones (el candado) + herramientas (las manos) ═══════════

  private configurarSesion(): void {
    const catalogo = BUSQUEDA_ENTIDADES.map(e => `- ${e.label} (clave: ${e.api})`).join('\n');
    const catalogoCore = SECCIONES_CORE.map(s => `- ${s.label} (clave: ${s.clave})`).join('\n');
    const apis = [...BUSQUEDA_ENTIDADES.map(e => e.api), ...SECCIONES_CORE.map(s => s.clave)];

    const instrucciones =
      `Sos el asistente de voz de "${this.siteConfig.sitioNombre()}", un sistema de gestión. ` +
      `SOLO ayudás a operarlo — si te hablan de cualquier otro tema respondé ` +
      `"yo solo te ayudo con tu sistema, ¿qué querés hacer?" y nada más. ` +
      `Hablá español rioplatense, PAUSADO y con calma, respuestas de UNA frase corta, siempre orientadas a la acción. ` +
      `NUNCA inventes datos ni ids: lo que no sepas se averigua con la herramienta buscar. ` +
      `Para abrir una ficha, primero buscá y usá el id real del resultado; si hay varios, preguntá cuál. ` +
      `ALTAS: usá abrir_alta — el resultado te da los CAMPOS del formulario (tipo, requerido, ` +
      `valores posibles). Apenas se abra decí: "Listo, decime el campo y el dato" — el USUARIO ` +
      `dicta a su ritmo (puede dar varios en una frase); vos llenás con llenar_campo lo que ` +
      `dictó, sin interrogar campo por campo. Si llenar_campo devuelve un problema (campo ` +
      `inexistente, valor inválido), DECILO EN EL MOMENTO en criollo y pedí el dato de nuevo. ` +
      `Cuando el usuario diga que terminó o pida guardar, revisá con estado_formulario: si ` +
      `falta algún requerido, nombralo puntualmente; si está completo, preguntá "¿lo guardo?". ` +
      `Campos tipo "ref": pedí opciones_de_referencia con la entidad del ref y elegí el ID ` +
      `de la opción que corresponde; si ninguna coincide claramente, leele al usuario las 2-3 ` +
      `más parecidas y preguntá. NUNCA pongas texto en un campo ref: va el ID numérico. ` +
      `Campos "enum": usá EXACTAMENTE uno de los valores listados. Fechas: formato AAAA-MM-DD. ` +
      `Cuando estén los campos requeridos, preguntá "¿lo guardo?" y SOLO llamá guardar si el ` +
      `usuario confirma con claridad. Si guardar devuelve error, explicalo en criollo y ofrecé corregir. ` +
      `EDICIONES: buscá el registro, llamá editar_registro con su id, cambiá lo pedido y confirmá antes de guardar. ` +
      `BORRAR: NO PODÉS — no tenés ninguna herramienta para eso; respondé "el borrado se hace a mano desde la pantalla". ` +
      `Las secciones del NEGOCIO son:\n${catalogo}\n` +
      `Y las secciones del SISTEMA BASE (administración) son:\n${catalogoCore}`;

    const evento = {
      type: 'session.update',
      session: {
        type: 'realtime',
        // La voz por defecto va apurada: bajar el ritmo la vuelve conversable
        audio: { output: { speed: 0.85 } },
        instructions: instrucciones,
        tools: [
          {
            type: 'function', name: 'navegar',
            description: 'Ir a la lista de una sección del sistema',
            parameters: { type: 'object', properties: { entidad: { type: 'string', enum: apis } }, required: ['entidad'] }
          },
          {
            type: 'function', name: 'buscar',
            description: 'Buscar registros por texto en todo el sistema; devuelve entidad, id y título de cada resultado',
            parameters: { type: 'object', properties: { texto: { type: 'string' } }, required: ['texto'] }
          },
          {
            type: 'function', name: 'abrir_ficha',
            description: 'Abrir la ficha de un registro (el id sale de un buscar previo, nunca se inventa)',
            parameters: { type: 'object', properties: { entidad: { type: 'string', enum: apis }, id: { type: 'number' } }, required: ['entidad', 'id'] }
          },
          { type: 'function', name: 'ir_inicio', description: 'Volver al inicio (los números del negocio)', parameters: { type: 'object', properties: {} } },
          { type: 'function', name: 'modo_pantalla', description: 'Poner el modo pantalla (vista para TV)', parameters: { type: 'object', properties: {} } },
          {
            type: 'function', name: 'abrir_alta',
            description: 'Abrir el formulario de alta de una entidad; devuelve sus campos (tipo, requerido, valores)',
            parameters: { type: 'object', properties: { entidad: { type: 'string', enum: BUSQUEDA_ENTIDADES.map(e => e.api) } }, required: ['entidad'] }
          },
          {
            type: 'function', name: 'editar_registro',
            description: 'Abrir la edición de un registro existente (el id sale de un buscar previo)',
            parameters: { type: 'object', properties: { entidad: { type: 'string', enum: BUSQUEDA_ENTIDADES.map(e => e.api) }, id: { type: 'number' } }, required: ['entidad', 'id'] }
          },
          {
            type: 'function', name: 'llenar_campo',
            description: 'Escribir un valor en un campo del formulario abierto (se ve en pantalla al instante)',
            parameters: { type: 'object', properties: { campo: { type: 'string' }, valor: {} }, required: ['campo', 'valor'] }
          },
          {
            type: 'function', name: 'estado_formulario',
            description: 'Ver qué campos tiene el formulario abierto, sus valores actuales y cuáles faltan',
            parameters: { type: 'object', properties: {} }
          },
          {
            type: 'function', name: 'guardar',
            description: 'Disparar el guardado REAL del formulario (solo tras confirmación verbal del usuario); devuelve éxito o el motivo del rechazo',
            parameters: { type: 'object', properties: {} }
          },
          {
            type: 'function', name: 'cancelar_formulario',
            description: 'Cerrar el formulario abierto sin guardar',
            parameters: { type: 'object', properties: {} }
          },
          {
            type: 'function', name: 'opciones_de_referencia',
            description: 'Listar las opciones de un catálogo referenciado (id + título) para elegir el ID de un campo ref. Rápido: usa caché de sesión.',
            parameters: { type: 'object', properties: { entidad: { type: 'string', enum: BUSQUEDA_ENTIDADES.map(e => e.api) } }, required: ['entidad'] }
          }
        ],
        tool_choice: 'auto'
      }
    };
    this.dc?.send(JSON.stringify(evento));
  }

  // ═══════════ Eventos del canal: estado + function calls ═══════════

  private onEvento(crudo: string): void {
    let ev: any;
    try { ev = JSON.parse(crudo); } catch { return; }

    switch (ev.type) {
      case 'input_audio_buffer.speech_started': this.estado.set('escuchando'); break;
      case 'response.created':
        this.respuestaActiva = true;
        this.estado.set('pensando');
        break;
      case 'response.output_audio.delta': this.estado.set('hablando'); break;
      case 'response.done': {
        this.respuestaActiva = false;
        this.estado.set('escuchando');
        const llamadas = (ev.response?.output || []).filter((i: any) => i.type === 'function_call');
        if (llamadas.length > 0) {
          this.ejecutarLote(llamadas);
        } else if (this.respuestaPendiente) {
          this.respuestaPendiente = false;
          this.solicitarRespuesta();
        }
        break;
      }
      case 'error':
        // El "active response in progress" es carrera de protocolo: reintentamos al done
        if ((ev.error?.message || '').includes('active response')) {
          this.respuestaPendiente = true;
          break;
        }
        console.error('[AsistenteVoz] error del proveedor:', ev);
        this.error.set(ev.error?.message || 'Error en la sesión de voz');
        break;
    }
  }

  /** Ejecuta TODAS las tools de la respuesta, manda todos los outputs y pide UNA
   *  sola respuesta nueva (dos response.create seguidos = error de protocolo). */
  private async ejecutarLote(llamadas: any[]): Promise<void> {
    for (const item of llamadas) {
      let args: any = {};
      try { args = JSON.parse(item.arguments || '{}'); } catch { /* args vacíos */ }

      let salida: any;
      try {
        salida = await this.correr(item.name, args);
      } catch (e: any) {
        salida = { error: e?.message || 'la acción falló' };
      }

      this.dc?.send(JSON.stringify({
        type: 'conversation.item.create',
        item: { type: 'function_call_output', call_id: item.call_id, output: JSON.stringify(salida) }
      }));
    }
    this.solicitarRespuesta();
  }

  private solicitarRespuesta(): void {
    if (this.respuestaActiva) { this.respuestaPendiente = true; return; }
    this.respuestaActiva = true; // optimista: el created llega enseguida
    this.dc?.send(JSON.stringify({ type: 'response.create' }));
  }

  private async correr(nombre: string, args: any): Promise<any> {
    const registro = (api: string) => BUSQUEDA_ENTIDADES.find(e => e.api === api);

    switch (nombre) {
      case 'navegar': {
        const e = registro(args.entidad);
        if (e) {
          this.ultimaAccion.set('→ ' + e.label);
          await this.router.navigate([e.ruta]);
          return { ok: true, pantalla: e.label };
        }
        const core = SECCIONES_CORE.find(s => s.clave === args.entidad);
        if (core) {
          this.ultimaAccion.set('→ ' + core.label);
          await this.router.navigate([core.ruta]);
          return { ok: true, pantalla: core.label };
        }
        return { error: 'esa sección no existe' };
      }
      case 'ir_inicio':
        this.ultimaAccion.set('→ Inicio');
        await this.router.navigate(['/']);
        return { ok: true, pantalla: 'inicio' };
      case 'modo_pantalla':
        this.ultimaAccion.set('→ Modo pantalla');
        await this.router.navigate(['/pantalla']);
        return { ok: true, pantalla: 'modo pantalla' };
      case 'abrir_ficha': {
        const e = registro(args.entidad);
        if (!e || !args.id) { return { error: 'falta la entidad o el id' }; }
        this.ultimaAccion.set('→ Ficha de ' + e.label + ' #' + args.id);
        await this.router.navigate([e.ruta, args.id]);
        return { ok: true, pantalla: 'ficha de ' + e.label + ' ' + args.id };
      }
      case 'buscar': {
        const q = (args.texto || '').toString().trim();
        if (q.length < 2) { return { resultados: [] }; }
        this.ultimaAccion.set('🔍 ' + q);
        // Timeout POR consulta: una entidad lenta no puede colgar la conversación —
        // se devuelve lo que llegó a tiempo (parcial honesto > silencio de 16s).
        const porEntidad = await Promise.all(BUSQUEDA_ENTIDADES.map(async (e) => {
          try {
            const items = await this.getConTimeout<any[]>(`${environment.apiUrl}/${e.api}/buscar`, { q }, 4000);
            return (items || []).slice(0, 3).map(i => ({
              entidad: e.api, id: i.id, titulo: i[e.campoTitulo] || (e.label + ' #' + i.id)
            }));
          } catch { return []; }
        }));
        return { resultados: porEntidad.flat().slice(0, 12) };
      }
      case 'abrir_alta': {
        const e = registro(args.entidad);
        if (!e) { return { error: 'esa sección no existe' }; }
        await this.router.navigate([e.ruta]);
        const abierto = await this.esperar(() => this.puente.abrirAlta(e.api), 3000);
        if (!abierto) { return { error: 'no pude abrir el formulario de alta' }; }
        const conForm = await this.esperar(() => this.puente.hayForm(), 3000);
        this.ultimaAccion.set('＋ Alta de ' + e.label);
        return conForm
          ? { ok: true, formulario: e.api, campos: e.campos }
          : { error: 'el formulario no llegó a abrirse' };
      }
      case 'editar_registro': {
        const e = registro(args.entidad);
        if (!e || !args.id) { return { error: 'falta la entidad o el id' }; }
        await this.router.navigate([e.ruta, args.id]);
        const ok = await this.esperar(() => this.puente.editarActual(e.api), 4000);
        if (!ok) { return { error: 'no pude abrir la edición' }; }
        await this.esperar(() => this.puente.hayForm(), 3000);
        this.ultimaAccion.set('✎ Editando ' + e.label + ' #' + args.id);
        return { ok: true, formulario: e.api, campos: e.campos, valores: this.puente.estadoForm() };
      }
      case 'llenar_campo': {
        const r = this.puente.llenarCampo(args.campo, args.valor);
        if (r.ok) { this.ultimaAccion.set('✎ ' + args.campo + ' = ' + String(args.valor)); }
        return r;
      }
      case 'estado_formulario':
        return { campos: this.puente.estadoForm() };
      case 'guardar': {
        this.ultimaAccion.set('💾 Guardando…');
        const r = await this.puente.guardar();
        this.ultimaAccion.set((r.ok ? '✔ ' : '✘ ') + r.detalle);
        return r;
      }
      case 'cancelar_formulario':
        this.puente.cancelar();
        this.ultimaAccion.set('formulario cancelado');
        return { ok: true };
      case 'opciones_de_referencia': {
        const e = registro(args.entidad);
        if (!e) { return { error: 'esa entidad no existe' }; }
        const cacheado = this.cacheRef.get(e.api);
        if (cacheado) { return { opciones: cacheado }; }
        try {
          const items = await this.getConTimeout<any[]>(`${environment.apiUrl}/${e.api}`, { take: '50' }, 6000);
          const opciones = (items || []).slice(0, 50).map(i => ({
            id: i.id, titulo: i[e.campoTitulo] || (e.label + ' #' + i.id)
          }));
          this.cacheRef.set(e.api, opciones);
          this.ultimaAccion.set('📋 opciones de ' + e.label);
          return { opciones };
        } catch {
          return { error: 'no pude traer las opciones de ' + e.label };
        }
      }
      default:
        return { error: 'herramienta desconocida' };
    }
  }

  /** GET con timeout duro: la voz no puede quedar rehén de una request colgada. */
  private getConTimeout<T>(url: string, params: Record<string, string>, ms: number): Promise<T> {
    return new Promise<T>((res, rej) => {
      const timer = setTimeout(() => { sub.unsubscribe(); rej(new Error('timeout')); }, ms);
      const sub = this.http.get<T>(url, { params }).subscribe({
        next: (v) => { clearTimeout(timer); res(v); },
        error: (e) => { clearTimeout(timer); rej(e); }
      });
    });
  }

  /** Espera activa corta: las pantallas se registran en el puente al renderizarse. */
  private esperar(cond: () => boolean, ms: number): Promise<boolean> {
    return new Promise((res) => {
      const t0 = Date.now();
      const intento = () => {
        let ok = false;
        try { ok = cond(); } catch { ok = false; }
        if (ok) { res(true); return; }
        if (Date.now() - t0 > ms) { res(false); return; }
        setTimeout(intento, 150);
      };
      intento();
    });
  }
}
