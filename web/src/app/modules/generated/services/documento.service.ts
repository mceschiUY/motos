import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { Documento, DocumentoConContenido } from '../models/documento.model';

@Injectable({
  providedIn: 'root'
})
export class DocumentoService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/Documentos`;

  getAll(): Observable<Documento[]> {
    return this.http.get<Documento[]>(this.baseUrl);
  }

  getById(id: number): Observable<DocumentoConContenido> {
    return this.http.get<DocumentoConContenido>(`${this.baseUrl}/${id}`);
  }

  getByRelacion(relacionId: number, relacionNombre: string): Observable<Documento[]> {
    return this.http.get<Documento[]>(`${this.baseUrl}/by-relacion`, {
      params: { relacionId: relacionId.toString(), relacionNombre }
    });
  }

  upload(file: File, relacionId: number, relacionNombre: string): Observable<number> {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('relacionId', relacionId.toString());
    formData.append('relacionNombre', relacionNombre);

    return this.http.post<number>(`${this.baseUrl}/upload`, formData);
  }

  delete(id: number): Observable<number> {
    return this.http.delete<number>(`${this.baseUrl}/${id}`);
  }

  getDownloadUrl(id: number): string {
    return `${this.baseUrl}/download/${id}`;
  }

  getFileIcon(extension: string): string {
    const ext = extension.toLowerCase().replace('.', '');
    const icons: { [key: string]: string } = {
      'pdf': 'picture_as_pdf',
      'png': 'image',
      'jpg': 'image',
      'jpeg': 'image',
      'gif': 'image',
      'bmp': 'image',
      'txt': 'description',
      'doc': 'article',
      'docx': 'article',
      'xls': 'table_chart',
      'xlsx': 'table_chart',
      'csv': 'table_chart',
      'msg': 'email',
      'eml': 'email',
      'zip': 'folder_zip',
      'rar': 'folder_zip',
      '7z': 'folder_zip',
      'mp3': 'audio_file',
      'wav': 'audio_file',
      'mp4': 'video_file',
      'avi': 'video_file',
      'mov': 'video_file'
    };
    return icons[ext] || 'insert_drive_file';
  }

  canPreview(extension: string): boolean {
    const ext = extension.toLowerCase().replace('.', '');
    const previewable = ['pdf', 'png', 'jpg', 'jpeg', 'gif', 'bmp', 'txt'];
    return previewable.includes(ext);
  }

  getPreviewType(extension: string): 'pdf' | 'image' | 'text' | 'none' {
    const ext = extension.toLowerCase().replace('.', '');
    if (ext === 'pdf') return 'pdf';
    if (['png', 'jpg', 'jpeg', 'gif', 'bmp'].includes(ext)) return 'image';
    if (ext === 'txt') return 'text';
    return 'none';
  }
}
