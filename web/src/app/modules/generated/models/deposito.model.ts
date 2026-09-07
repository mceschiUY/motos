export interface Deposito {
  id: number;
  codigo: string;
  nombre: string;
  direccion?: string | null;
  activo: boolean;
}
