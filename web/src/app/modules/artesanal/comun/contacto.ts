/**
 * Contacto de un tap (Etapa H.10): los links que el vendedor toca desde el celular. Un solo
 * lugar para "llamar", "WhatsApp" y "cómo llegar", así la agenda, Mis clientes, Mis pedidos y
 * las fichas arman los mismos links. Sin dependencias.
 */

/** Dígitos del teléfono para `wa.me` (sin espacios, guiones ni "+"); null si no alcanza para un número. */
export function digitosWhatsapp(telefono: string | null | undefined): string | null {
  const d = (telefono ?? '').replace(/[^0-9]/g, '');
  return d.length >= 6 ? d : null;
}

/** `https://wa.me/598…?text=…`; null si no hay teléfono usable. */
export function whatsappUrl(telefono: string | null | undefined, texto?: string): string | null {
  const d = digitosWhatsapp(telefono);
  if (!d) return null;
  return `https://wa.me/${d}` + (texto ? `?text=${encodeURIComponent(texto)}` : '');
}

/** `tel:+598…`; null si no hay teléfono. */
export function telUrl(telefono: string | null | undefined): string | null {
  const t = (telefono ?? '').replace(/[^0-9+]/g, '');
  return t.length >= 6 ? `tel:${t}` : null;
}

/** Google Maps por coordenadas o, si no hay, por dirección y ciudad; null si no hay nada. */
export function mapaUrl(lat: number | null | undefined, lng: number | null | undefined,
                        direccion?: string | null, ciudad?: string | null): string | null {
  if (lat != null && lng != null) {
    return `https://www.google.com/maps/search/?api=1&query=${lat},${lng}`;
  }
  const q = [direccion, ciudad].filter(x => !!x && x.trim()).join(', ');
  return q ? `https://www.google.com/maps/search/?api=1&query=${encodeURIComponent(q)}` : null;
}

/** Abre un link de contacto: `tel:` en la misma pestaña, el resto en una nueva. */
export function abrirContacto(url: string | null): void {
  if (!url) return;
  if (url.startsWith('tel:')) { window.location.href = url; return; }
  window.open(url, '_blank', 'noopener');
}
