import { bootstrapApplication } from '@angular/platform-browser';
import { appConfig } from './app/app.config';
import { AppComponent } from './app/app.component';
import './app/shared/confirm/confirmar'; // registra window.confirmar (diálogo del sistema)

bootstrapApplication(AppComponent, appConfig)
  .catch((err) => console.error(err));
