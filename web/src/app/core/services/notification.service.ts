import { Injectable, inject } from '@angular/core';
import { MatSnackBar, MatSnackBarConfig } from '@angular/material/snack-bar';

export type NotificationType = 'success' | 'error' | 'warning' | 'info';

export interface NotificationOptions {
  duration?: number;
  action?: string;
  type?: NotificationType;
}

/**
 * Servicio de Notificaciones Centralizado
 * Proporciona metodos para mostrar notificaciones consistentes en toda la aplicacion.
 * Compatible con componentes generados - pueden usarlo opcionalmente.
 */
@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private readonly snackBar = inject(MatSnackBar);

  private readonly defaultConfig: MatSnackBarConfig = {
    duration: 4000,
    horizontalPosition: 'end',
    verticalPosition: 'top'
  };

  private readonly panelClasses: Record<NotificationType, string> = {
    success: 'notification-success',
    error: 'notification-error',
    warning: 'notification-warning',
    info: 'notification-info'
  };

  /**
   * Muestra una notificacion generica
   */
  show(message: string, options: NotificationOptions = {}): void {
    const config: MatSnackBarConfig = {
      ...this.defaultConfig,
      duration: options.duration ?? this.defaultConfig.duration,
      panelClass: options.type ? this.panelClasses[options.type] : undefined
    };

    this.snackBar.open(message, options.action || 'Cerrar', config);
  }

  /**
   * Notificacion de exito
   */
  success(message: string, action?: string): void {
    this.show(message, { type: 'success', action });
  }

  /**
   * Notificacion de error
   */
  error(message: string, action?: string): void {
    this.show(message, { type: 'error', action, duration: 6000 });
  }

  /**
   * Notificacion de advertencia
   */
  warning(message: string, action?: string): void {
    this.show(message, { type: 'warning', action });
  }

  /**
   * Notificacion informativa
   */
  info(message: string, action?: string): void {
    this.show(message, { type: 'info', action });
  }

  /**
   * Muestra error de API con mensaje amigable
   */
  apiError(error: any): void {
    let message = 'Ha ocurrido un error inesperado';

    if (error?.error?.detail) {
      message = error.error.detail;
    } else if (error?.error?.message) {
      message = error.error.message;
    } else if (error?.message) {
      message = error.message;
    } else if (typeof error === 'string') {
      message = error;
    }

    // Mensajes especificos por codigo HTTP
    if (error?.status) {
      switch (error.status) {
        case 400:
          message = error?.error?.detail || 'Datos invalidos';
          break;
        case 401:
          message = 'Sesion expirada. Por favor, inicie sesion nuevamente';
          break;
        case 403:
          message = 'No tiene permisos para realizar esta accion';
          break;
        case 404:
          message = 'El recurso solicitado no existe';
          break;
        case 422:
          message = error?.error?.detail || 'Error de validacion';
          break;
        case 500:
          message = 'Error interno del servidor';
          break;
        case 0:
          message = 'No se pudo conectar con el servidor';
          break;
      }
    }

    this.error(message);
  }

  /**
   * Cierra todas las notificaciones activas
   */
  dismiss(): void {
    this.snackBar.dismiss();
  }
}
