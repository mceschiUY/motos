import { Pipe, PipeTransform } from '@angular/core';

const FORMATOS = new Map<number, Intl.NumberFormat>();

/**
 * Formato ÚNICO de dólares para las escenas artesanales (revisión 2026-09-12): "US$ 1.234,50"
 * con separadores de es-UY. Antes convivían `'US$ ' + Intl`, `US$ {{ x | number }}` (en-US:
 * "1,234.50") y `currency:'USD'`. Solo dólares: no hay moneda ni tipo de cambio en el dominio.
 */
export function usd(valor: number | null | undefined, decimales = 2): string {
  const n = valor ?? 0;
  let f = FORMATOS.get(decimales);
  if (!f) {
    f = new Intl.NumberFormat('es-UY', { minimumFractionDigits: decimales, maximumFractionDigits: decimales });
    FORMATOS.set(decimales, f);
  }
  return 'US$ ' + f.format(n);
}

/** `{{ total | usd }}` → "US$ 1.234,50" · `{{ total | usd:0 }}` → "US$ 1.235". */
@Pipe({ name: 'usd', standalone: true })
export class UsdPipe implements PipeTransform {
  transform(valor: number | null | undefined, decimales = 2): string { return usd(valor, decimales); }
}
