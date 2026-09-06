import { inject } from '@angular/core';
import {
  HttpInterceptorFn,
  HttpRequest,
  HttpHandlerFn,
  HttpErrorResponse
} from '@angular/common/http';
import { catchError, finalize, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { LoadingService } from '../services/loading.service';
import { NotificationService } from '../services/notification.service';
import { Router } from '@angular/router';

/**
 * Interceptor HTTP Global
 *
 * Funcionalidades:
 * 1. Agrega automaticamente el token de autenticacion a todas las peticiones
 * 2. Controla el loading global automaticamente
 * 3. Maneja errores HTTP de forma centralizada
 * 4. Redirige a login cuando el token expira (401)
 *
 * Compatible con todos los componentes generados - funciona transparentemente.
 */
export const httpInterceptor: HttpInterceptorFn = (req: HttpRequest<unknown>, next: HttpHandlerFn) => {
  const authService = inject(AuthService);
  const loadingService = inject(LoadingService);
  const notificationService = inject(NotificationService);
  const router = inject(Router);

  // URLs que no requieren loading (opcionales, evita parpadeos)
  const skipLoadingUrls = ['/api/health', '/api/ping'];
  const showLoading = !skipLoadingUrls.some(url => req.url.includes(url));

  // URLs que no requieren autenticacion
  const publicUrls = ['/api/auth/login', '/api/auth/register', '/api/auth/forgot-password'];
  const isPublicUrl = publicUrls.some(url => req.url.includes(url));

  // Clonar request con headers de autenticacion
  let authReq = req;
  const token = authService.token();

  if (token && !isPublicUrl) {
    authReq = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });
  }

  // Iniciar loading
  if (showLoading) {
    loadingService.startRequest();
  }

  return next(authReq).pipe(
    catchError((error: HttpErrorResponse) => {
      // Manejar errores especificos
      switch (error.status) {
        case 401:
          // Token expirado o invalido
          authService.logout();
          notificationService.warning('Su sesion ha expirado. Por favor, inicie sesion nuevamente.');
          router.navigate(['/login']);
          break;

        case 403:
          // Sin permisos
          notificationService.error('No tiene permisos para realizar esta accion');
          break;

        case 0:
          // Error de conexion
          notificationService.error('No se pudo conectar con el servidor. Verifique su conexion.');
          break;

        // Los errores 400, 422, 500 se manejan en los componentes individuales
        // para mostrar mensajes mas especificos si es necesario
      }

      return throwError(() => error);
    }),
    finalize(() => {
      // Finalizar loading
      if (showLoading) {
        loadingService.endRequest();
      }
    })
  );
};
