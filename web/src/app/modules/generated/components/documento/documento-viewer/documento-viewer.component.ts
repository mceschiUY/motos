import { Component, Inject, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';

import { Documento, DocumentoConContenido } from '../../../models/documento.model';
import { DocumentoService } from '../../../services/documento.service';

@Component({
  selector: 'app-documento-viewer',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatTooltipModule
  ],
  templateUrl: './documento-viewer.component.html',
  styleUrl: './documento-viewer.component.scss'
})
export class DocumentoViewerComponent implements OnInit {
  private readonly service = inject(DocumentoService);
  private readonly sanitizer = inject(DomSanitizer);

  readonly isLoading = signal<boolean>(true);
  readonly previewType = signal<'pdf' | 'image' | 'text' | 'none'>('none');
  readonly textContent = signal<string>('');
  readonly imageSrc = signal<SafeResourceUrl | null>(null);
  readonly pdfSrc = signal<SafeResourceUrl | null>(null);
  readonly errorMessage = signal<string>('');

  constructor(
    public dialogRef: MatDialogRef<DocumentoViewerComponent>,
    @Inject(MAT_DIALOG_DATA) public data: Documento
  ) {}

  ngOnInit(): void {
    this.previewType.set(this.service.getPreviewType(this.data.extension));
    this.loadContent();
  }

  private loadContent(): void {
    this.service.getById(this.data.id).subscribe({
      next: (doc: DocumentoConContenido) => {
        this.processContent(doc);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('[DocumentoViewer] Error cargando contenido:', err);
        this.errorMessage.set('Error al cargar el documento');
        this.isLoading.set(false);
      }
    });
  }

  private processContent(doc: DocumentoConContenido): void {
    const type = this.previewType();

    switch (type) {
      case 'pdf':
        const pdfBlob = this.base64ToBlob(doc.contenido, 'application/pdf');
        const pdfUrl = URL.createObjectURL(pdfBlob);
        this.pdfSrc.set(this.sanitizer.bypassSecurityTrustResourceUrl(pdfUrl));
        break;

      case 'image':
        const imageUrl = `data:${doc.mimeType};base64,${doc.contenido}`;
        this.imageSrc.set(this.sanitizer.bypassSecurityTrustResourceUrl(imageUrl));
        break;

      case 'text':
        try {
          this.textContent.set(atob(doc.contenido));
        } catch {
          this.textContent.set('Error al decodificar el contenido de texto');
        }
        break;

      default:
        this.errorMessage.set('Este tipo de archivo no se puede previsualizar');
        break;
    }
  }

  private base64ToBlob(base64: string, mimeType: string): Blob {
    const byteCharacters = atob(base64);
    const byteNumbers = new Array(byteCharacters.length);
    for (let i = 0; i < byteCharacters.length; i++) {
      byteNumbers[i] = byteCharacters.charCodeAt(i);
    }
    const byteArray = new Uint8Array(byteNumbers);
    return new Blob([byteArray], { type: mimeType });
  }

  getFileIcon(): string {
    return this.service.getFileIcon(this.data.extension);
  }

  download(): void {
    const url = this.service.getDownloadUrl(this.data.id);
    window.open(url, '_blank');
  }

  close(): void {
    this.dialogRef.close();
  }
}
