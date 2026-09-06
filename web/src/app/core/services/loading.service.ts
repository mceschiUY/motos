import { Injectable, signal, computed } from '@angular/core';

/**
 * Servicio de Loading Global
 * Controla el estado de carga de la aplicacion.
 * Compatible con el interceptor HTTP para mostrar loading automaticamente.
 */
@Injectable({
  providedIn: 'root'
})
export class LoadingService {
  // Contador de peticiones activas
  private activeRequests = signal(0);

  // Estado de loading derivado
  readonly isLoading = computed(() => this.activeRequests() > 0);

  // Loading manual (para operaciones no-HTTP)
  private manualLoading = signal(false);

  // Loading combinado
  readonly showLoading = computed(() => this.isLoading() || this.manualLoading());

  /**
   * Incrementa el contador de peticiones activas
   * Llamado automaticamente por el interceptor HTTP
   */
  startRequest(): void {
    this.activeRequests.update(count => count + 1);
  }

  /**
   * Decrementa el contador de peticiones activas
   * Llamado automaticamente por el interceptor HTTP
   */
  endRequest(): void {
    this.activeRequests.update(count => Math.max(0, count - 1));
  }

  /**
   * Activa loading manual (para operaciones no-HTTP)
   */
  show(): void {
    this.manualLoading.set(true);
  }

  /**
   * Desactiva loading manual
   */
  hide(): void {
    this.manualLoading.set(false);
  }

  /**
   * Resetea todos los estados de loading
   */
  reset(): void {
    this.activeRequests.set(0);
    this.manualLoading.set(false);
  }
}
