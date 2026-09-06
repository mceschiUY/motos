import { Directive, Input, TemplateRef, ViewContainerRef, inject, effect } from '@angular/core';
import { AuthService } from '../services/auth.service';

/**
 * Directiva para mostrar/ocultar elementos basado en capabilities del usuario.
 *
 * Uso:
 * ```html
 * <button *hasCapability="'Usuario.Crear'">Crear Usuario</button>
 * <div *hasAnyCapability="['Admin.Full', 'Usuario.Ver']">Contenido</div>
 * ```
 *
 * Compatible con componentes generados - uso opcional.
 */
@Directive({
  selector: '[hasCapability]',
  standalone: true
})
export class HasCapabilityDirective {
  private readonly authService = inject(AuthService);
  private readonly templateRef = inject(TemplateRef<any>);
  private readonly viewContainer = inject(ViewContainerRef);

  private hasView = false;

  @Input() set hasCapability(capability: string) {
    this.updateView(this.authService.hasCapability(capability));
  }

  private updateView(condition: boolean): void {
    if (condition && !this.hasView) {
      this.viewContainer.createEmbeddedView(this.templateRef);
      this.hasView = true;
    } else if (!condition && this.hasView) {
      this.viewContainer.clear();
      this.hasView = false;
    }
  }
}

/**
 * Directiva para mostrar elemento si tiene ALGUNA de las capabilities.
 */
@Directive({
  selector: '[hasAnyCapability]',
  standalone: true
})
export class HasAnyCapabilityDirective {
  private readonly authService = inject(AuthService);
  private readonly templateRef = inject(TemplateRef<any>);
  private readonly viewContainer = inject(ViewContainerRef);

  private hasView = false;

  @Input() set hasAnyCapability(capabilities: string[]) {
    const hasAny = capabilities.some(cap => this.authService.hasCapability(cap));
    this.updateView(hasAny);
  }

  private updateView(condition: boolean): void {
    if (condition && !this.hasView) {
      this.viewContainer.createEmbeddedView(this.templateRef);
      this.hasView = true;
    } else if (!condition && this.hasView) {
      this.viewContainer.clear();
      this.hasView = false;
    }
  }
}

/**
 * Directiva para mostrar elemento si tiene TODAS las capabilities.
 */
@Directive({
  selector: '[hasAllCapabilities]',
  standalone: true
})
export class HasAllCapabilitiesDirective {
  private readonly authService = inject(AuthService);
  private readonly templateRef = inject(TemplateRef<any>);
  private readonly viewContainer = inject(ViewContainerRef);

  private hasView = false;

  @Input() set hasAllCapabilities(capabilities: string[]) {
    const hasAll = capabilities.every(cap => this.authService.hasCapability(cap));
    this.updateView(hasAll);
  }

  private updateView(condition: boolean): void {
    if (condition && !this.hasView) {
      this.viewContainer.createEmbeddedView(this.templateRef);
      this.hasView = true;
    } else if (!condition && this.hasView) {
      this.viewContainer.clear();
      this.hasView = false;
    }
  }
}
