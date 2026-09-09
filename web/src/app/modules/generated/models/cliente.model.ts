export interface Cliente {
  id: number;
  nombre: string;
  telefono: string;
  direccionEntrega: string;
  /** tienda | distribuidor | online | particular */
  tipo?: string | null;
  ciudad?: string | null;
  contacto?: string | null;
  email?: string | null;
  vendedorId?: number | null;
  vendedorDisplay?: string;
  notas?: string | null;
  latitud?: number | null;
  longitud?: number | null;
}
