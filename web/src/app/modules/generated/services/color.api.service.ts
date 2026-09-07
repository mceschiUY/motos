import { Injectable } from '@angular/core';
import { GeneratedApiBase } from '../../../core/services/generated-api.base';
import { Color } from '../models/color.model';

@Injectable({
  providedIn: 'root'
})
export class ColorApiService extends GeneratedApiBase<Color> {
  protected readonly recurso = 'Color';
}
