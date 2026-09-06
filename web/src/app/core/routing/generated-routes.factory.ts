import { Type } from '@angular/core';
import { Routes } from '@angular/router';

// ─── Factory de rutas de entidad generada (Fase A1.3) ───────────────────────
// Patrón fijo list + ficha. Los import() lazy viajan como thunks LITERALES
// desde el archivo generado — el bundler necesita el string estático para
// hacer code-splitting; acá solo se arma la estructura.

type CargaComponente = () => Promise<Type<unknown>>;

export function rutasDeEntidad(list: CargaComponente, ficha?: CargaComponente): Routes {
  const rutas: Routes = [{ path: '', loadComponent: list }];
  if (ficha) {
    rutas.push({ path: ':id', loadComponent: ficha });
  }
  return rutas;
}
