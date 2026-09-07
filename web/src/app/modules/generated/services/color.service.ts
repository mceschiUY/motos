import { Injectable, inject } from '@angular/core';
import { GeneratedFacadeBase } from '../../../core/services/generated-facade.base';
import { Color } from '../models/color.model';
import { ColorApiService } from './color.api.service';

@Injectable({
  providedIn: 'root'
})
export class ColorService extends GeneratedFacadeBase<Color, ColorApiService> {
  protected readonly apiService = inject(ColorApiService);
}
