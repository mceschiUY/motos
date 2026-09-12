import { AfterViewInit, Component, ElementRef, ViewChild, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar } from '@angular/material/snack-bar';
import QRCode from 'qrcode';
import { environment } from '../../../../environments/environment';

interface LanInfo { ips: string[]; puertoApi: number; host: string; }

/**
 * "Abrir en el celular" (plan Etapa F2, 2026-09-12): diálogo con el QR de la URL del sitio en
 * la IP de la máquina, para escanearlo desde el teléfono en la misma WiFi. Las IPs las da
 * `GET api/artesanal/lan` (solo Development). Sin backend o en producción, usa la URL actual.
 */
@Component({
  selector: 'app-abrir-en-celular',
  standalone: true,
  imports: [CommonModule, MatDialogModule, MatButtonModule, MatIconModule],
  template: `
    <h2 mat-dialog-title><mat-icon>smartphone</mat-icon> Abrir en el celular</h2>
    <mat-dialog-content class="aec">
      @if (urls().length > 1) {
        <div class="aec-ips">
          @for (u of urls(); track u) {
            <button type="button" class="aec-ip" [class.activa]="u === url()" (click)="elegir(u)">{{ u }}</button>
          }
        </div>
      }
      <div class="aec-qr"><canvas #canvas></canvas></div>
      <p class="aec-url">{{ url() || '…' }}</p>
      <p class="aec-ayuda">
        Escaneá el código con el teléfono <strong>en la misma WiFi</strong>. Al abrirlo, el
        navegador ofrece "Agregar a la pantalla de inicio" y queda instalado como app.
      </p>
      @if (aviso()) { <p class="aec-aviso"><mat-icon>info</mat-icon> {{ aviso() }}</p> }
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button (click)="copiar()"><mat-icon>content_copy</mat-icon> Copiar link</button>
      <button mat-flat-button color="primary" (click)="ref.close()">Listo</button>
    </mat-dialog-actions>
  `,
  styles: [`
    h2 { display: flex; align-items: center; gap: 8px; }
    .aec { display: flex; flex-direction: column; align-items: center; gap: 12px; min-width: 280px; }
    .aec-qr { padding: 12px; background: #fff; border-radius: var(--ceskia-radius-lg); }
    .aec-url { font-family: var(--ceskia-font-mono); font-size: .95rem; margin: 0; word-break: break-all; text-align: center; }
    .aec-ayuda { color: var(--ceskia-text-secondary); font-size: .9rem; margin: 0; text-align: center; max-width: 340px; }
    .aec-aviso { display: flex; gap: 6px; align-items: center; color: var(--ceskia-accent-warning); font-size: .85rem; margin: 0; text-align: center; }
    .aec-ips { display: flex; flex-wrap: wrap; gap: 6px; justify-content: center; }
    .aec-ip { border: 1px solid var(--ceskia-border-default); background: var(--ceskia-surface); color: var(--ceskia-text-secondary); border-radius: var(--ceskia-radius-full); padding: 4px 10px; font-size: .8rem; cursor: pointer; }
    .aec-ip.activa { border-color: var(--ceskia-accent-primary); color: var(--ceskia-accent-primary); }
  `]
})
export class AbrirEnCelularComponent implements AfterViewInit {
  readonly ref = inject(MatDialogRef<AbrirEnCelularComponent>);
  private readonly http = inject(HttpClient);
  private readonly snack = inject(MatSnackBar);

  @ViewChild('canvas') canvas!: ElementRef<HTMLCanvasElement>;

  readonly urls = signal<string[]>([]);
  readonly url = signal('');
  readonly aviso = signal('');

  ngAfterViewInit(): void {
    const puerto = window.location.port ? `:${window.location.port}` : '';
    const ruta = window.location.pathname + window.location.search;
    this.http.get<LanInfo>(`${environment.apiUrl}/artesanal/lan`).subscribe({
      next: (info) => {
        const urls = (info.ips ?? []).map(ip => `http://${ip}${puerto}${ruta}`);
        if (urls.length === 0) {
          this.aviso.set('No encontré una IP de red local: el celular tiene que estar en la misma WiFi que esta máquina.');
          urls.push(window.location.href);
        }
        if (window.location.protocol === 'https:') {
          this.aviso.set('Abriste el sitio por HTTPS: en el celular usá HTTP (el certificado de desarrollo no vale ahí).');
        }
        this.urls.set(urls);
        this.elegir(urls[0]);
      },
      error: () => {
        this.aviso.set('No pude consultar la IP de la máquina (¿la API corre con el perfil "lan"?). Uso la URL actual.');
        this.urls.set([window.location.href]);
        this.elegir(window.location.href);
      },
    });
  }

  elegir(u: string): void {
    this.url.set(u);
    QRCode.toCanvas(this.canvas.nativeElement, u, { width: 220, margin: 1 }).catch((e: unknown) => console.error('[AbrirEnCelular] QR:', e));
  }

  copiar(): void {
    navigator.clipboard?.writeText(this.url()).then(
      () => this.snack.open('Link copiado', undefined, { duration: 2000 }),
      () => this.snack.open('No se pudo copiar', undefined, { duration: 2000 }),
    );
  }
}
