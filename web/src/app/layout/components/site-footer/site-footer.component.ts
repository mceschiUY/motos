import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { SiteConfigService } from '../../../core/services/site-config.service';

@Component({
  selector: 'app-site-footer',
  standalone: true,
  imports: [CommonModule, MatIconModule, MatButtonModule, MatTooltipModule],
  templateUrl: './site-footer.component.html',
  styleUrl: './site-footer.component.scss'
})
export class SiteFooterComponent {
  readonly siteConfig = inject(SiteConfigService);

  // Helpers para template
  get hasContacto(): boolean {
    return this.siteConfig.hasContacto();
  }

  get hasRedes(): boolean {
    return this.siteConfig.hasRedesSociales();
  }

  openWhatsApp(): void {
    const numero = this.siteConfig.socialWhatsapp();
    if (numero) {
      // Limpiar numero (solo digitos)
      const cleanNumber = numero.replace(/\D/g, '');
      window.open(`https://wa.me/${cleanNumber}`, '_blank');
    }
  }

  openLink(url: string): void {
    if (url) {
      window.open(url, '_blank');
    }
  }

  sendEmail(): void {
    const email = this.siteConfig.empresaEmail();
    if (email) {
      window.location.href = `mailto:${email}`;
    }
  }
}
