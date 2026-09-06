import { Component, Inject, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';

import { DocumentoService } from '../../../services/documento.service';

export interface DocumentoUploadData {
  relacionId: number;
  relacionNombre: string;
}

@Component({
  selector: 'app-documento-upload',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    MatProgressBarModule
  ],
  templateUrl: './documento-upload.component.html',
  styleUrl: './documento-upload.component.scss'
})
export class DocumentoUploadComponent {
  private readonly service = inject(DocumentoService);

  readonly isUploading = signal<boolean>(false);
  readonly uploadProgress = signal<number>(0);
  readonly selectedFile = signal<File | null>(null);
  readonly errorMessage = signal<string>('');
  readonly isDragOver = signal<boolean>(false);

  constructor(
    public dialogRef: MatDialogRef<DocumentoUploadComponent>,
    @Inject(MAT_DIALOG_DATA) public data: DocumentoUploadData
  ) {}

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragOver.set(true);
  }

  onDragLeave(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragOver.set(false);
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragOver.set(false);

    const files = event.dataTransfer?.files;
    if (files && files.length > 0) {
      this.selectFile(files[0]);
    }
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      this.selectFile(input.files[0]);
    }
  }

  private selectFile(file: File): void {
    this.errorMessage.set('');

    // Validar tamano (max 50MB)
    const maxSize = 50 * 1024 * 1024;
    if (file.size > maxSize) {
      this.errorMessage.set('El archivo excede el tamano maximo de 50MB');
      return;
    }

    this.selectedFile.set(file);
  }

  getFileIcon(): string {
    const file = this.selectedFile();
    if (!file) return 'insert_drive_file';

    const ext = '.' + file.name.split('.').pop()?.toLowerCase();
    return this.service.getFileIcon(ext);
  }

  formatFileSize(bytes: number): string {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  }

  removeFile(): void {
    this.selectedFile.set(null);
    this.errorMessage.set('');
  }

  upload(): void {
    const file = this.selectedFile();
    if (!file) return;

    this.isUploading.set(true);
    this.uploadProgress.set(0);
    this.errorMessage.set('');

    // Simular progreso mientras se sube
    const progressInterval = setInterval(() => {
      const current = this.uploadProgress();
      if (current < 90) {
        this.uploadProgress.set(current + 10);
      }
    }, 200);

    this.service.upload(file, this.data.relacionId, this.data.relacionNombre).subscribe({
      next: (id) => {
        clearInterval(progressInterval);
        this.uploadProgress.set(100);
        setTimeout(() => {
          this.dialogRef.close(true);
        }, 300);
      },
      error: (err) => {
        clearInterval(progressInterval);
        console.error('[DocumentoUpload] Error:', err);
        this.errorMessage.set('Error al subir el archivo. Intenta de nuevo.');
        this.isUploading.set(false);
        this.uploadProgress.set(0);
      }
    });
  }

  cancel(): void {
    this.dialogRef.close(false);
  }
}
