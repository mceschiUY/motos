/**
 * Confirmación del sistema — reemplaza al confirm() nativo del navegador con un
 * diálogo con la estética Ceskia (tokens del design system, teclado y backdrop).
 *
 * Sin DI a propósito: los componentes generados lo invocan vía
 * `(window as any).confirmar(...)` sin necesitar imports; el código de mano
 * puede importar { confirmar } directamente. Se registra en window al cargar
 * el módulo (main.ts lo importa por side-effect).
 */

export interface OpcionesConfirmar {
  mensaje: string;
  titulo?: string;
  textoConfirmar?: string;
  textoCancelar?: string;
  /** Pinta el botón de confirmar en rojo. Si se omite, se infiere del mensaje. */
  peligro?: boolean;
}

const CSS_ID = 'ceskia-confirmar-css';

const CSS = `
.ck-confirm-backdrop { position: fixed; inset: 0; z-index: 10000;
  background: rgba(6, 10, 18, 0.62); backdrop-filter: blur(3px);
  display: flex; align-items: center; justify-content: center;
  animation: ckConfirmFade 0.14s ease; }
.ck-confirm-caja { min-width: 320px; max-width: 440px; margin: 16px;
  background: var(--ceskia-elevated);
  border: 1px solid var(--ceskia-border-strong);
  border-radius: 14px; padding: 22px 24px 18px;
  box-shadow: var(--ceskia-shadow-xl);
  animation: ckConfirmPop 0.16s ease; }
.ck-confirm-titulo { margin: 0 0 8px; font-size: 15px; font-weight: 700;
  color: var(--ceskia-text-primary); }
.ck-confirm-mensaje { margin: 0 0 18px; font-size: 13.5px; line-height: 1.5;
  color: var(--ceskia-text-secondary); white-space: pre-line; }
.ck-confirm-botones { display: flex; gap: 10px; justify-content: flex-end; }
.ck-confirm-btn { padding: 8px 16px; border-radius: 9px; font-size: 13px;
  font-weight: 600; cursor: pointer; transition: all 0.15s ease;
  border: 1px solid var(--ceskia-border-default);
  background: transparent; color: var(--ceskia-text-secondary); }
.ck-confirm-btn:hover { color: var(--ceskia-text-primary);
  border-color: var(--ceskia-border-strong); }
.ck-confirm-btn.ok { border: none; color: #ffffff;
  background: var(--ceskia-accent-primary); }
.ck-confirm-btn.ok:hover { background: var(--ceskia-accent-primary-hover);
  box-shadow: 0 0 14px var(--ceskia-accent-primary-glow); }
.ck-confirm-btn.ok.peligro { background: var(--ceskia-accent-danger); }
.ck-confirm-btn.ok.peligro:hover { background: var(--ceskia-accent-danger-hover);
  box-shadow: 0 0 14px var(--ceskia-accent-danger-glow); }
@keyframes ckConfirmFade { from { opacity: 0; } to { opacity: 1; } }
@keyframes ckConfirmPop { from { opacity: 0; transform: scale(0.96) translateY(6px); }
  to { opacity: 1; transform: none; } }
`;

function asegurarCss(): void {
  if (document.getElementById(CSS_ID)) return;
  const style = document.createElement('style');
  style.id = CSS_ID;
  style.textContent = CSS;
  document.head.appendChild(style);
}

export function confirmar(opciones: string | OpcionesConfirmar): Promise<boolean> {
  const op: OpcionesConfirmar = typeof opciones === 'string' ? { mensaje: opciones } : opciones;
  const peligro = op.peligro ?? /elimina|desactiv|borra|quitar|cancelar\b/i.test(op.mensaje);

  asegurarCss();

  return new Promise<boolean>(resolve => {
    const backdrop = document.createElement('div');
    backdrop.className = 'ck-confirm-backdrop';

    const caja = document.createElement('div');
    caja.className = 'ck-confirm-caja';
    caja.setAttribute('role', 'alertdialog');
    caja.setAttribute('aria-modal', 'true');

    const titulo = document.createElement('h3');
    titulo.className = 'ck-confirm-titulo';
    titulo.textContent = op.titulo ?? (peligro ? 'Confirmar acción' : 'Confirmar');

    const mensaje = document.createElement('p');
    mensaje.className = 'ck-confirm-mensaje';
    mensaje.textContent = op.mensaje;

    const botones = document.createElement('div');
    botones.className = 'ck-confirm-botones';

    const btnCancelar = document.createElement('button');
    btnCancelar.type = 'button';
    btnCancelar.className = 'ck-confirm-btn';
    btnCancelar.textContent = op.textoCancelar ?? 'Cancelar';

    const btnOk = document.createElement('button');
    btnOk.type = 'button';
    btnOk.className = 'ck-confirm-btn ok' + (peligro ? ' peligro' : '');
    btnOk.textContent = op.textoConfirmar ?? (peligro ? 'Sí, continuar' : 'Aceptar');

    const focoPrevio = document.activeElement as HTMLElement | null;

    const cerrar = (resultado: boolean) => {
      document.removeEventListener('keydown', onTecla, true);
      backdrop.remove();
      focoPrevio?.focus?.();
      resolve(resultado);
    };

    const onTecla = (ev: KeyboardEvent) => {
      if (ev.key === 'Escape') { ev.preventDefault(); ev.stopPropagation(); cerrar(false); }
      else if (ev.key === 'Enter') { ev.preventDefault(); ev.stopPropagation(); cerrar(true); }
    };

    btnCancelar.addEventListener('click', () => cerrar(false));
    btnOk.addEventListener('click', () => cerrar(true));
    backdrop.addEventListener('click', ev => { if (ev.target === backdrop) cerrar(false); });
    document.addEventListener('keydown', onTecla, true);

    botones.append(btnCancelar, btnOk);
    caja.append(titulo, mensaje, botones);
    backdrop.appendChild(caja);
    document.body.appendChild(backdrop);
    // El foco arranca en Cancelar: Enter confirma a propósito, no por accidente.
    btnCancelar.focus();
  });
}

// Disponible globalmente para los componentes generados (sin import).
(window as unknown as { confirmar: typeof confirmar }).confirmar = confirmar;
