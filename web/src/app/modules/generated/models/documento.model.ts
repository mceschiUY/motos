export interface Documento {
  id: number;
  nombre: string;
  extension: string;
  mimeType: string;
  fechaCarga: Date;
  relacionId: number;
  relacionNombre: string;
}

export interface DocumentoConContenido extends Documento {
  contenido: string; // Base64
}

export interface DocumentoUpload {
  file: File;
  relacionId: number;
  relacionNombre: string;
}
