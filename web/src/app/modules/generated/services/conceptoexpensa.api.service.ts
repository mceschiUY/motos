import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { Conceptoexpensa } from '../models/conceptoexpensa.model';

@Injectable({
  providedIn: 'root'
})
export class ConceptoexpensaApiService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/Conceptoexpensa`;

  getAll(): Observable<Conceptoexpensa[]> {
    return this.http.get<Conceptoexpensa[]>(this.apiUrl);
  }

  getById(id: string): Observable<Conceptoexpensa> {
    return this.http.get<Conceptoexpensa>(`${this.apiUrl}/${id}`);
  }

  create(data: Omit<Conceptoexpensa, 'id'>): Observable<Conceptoexpensa> {
    return this.http.post<Conceptoexpensa>(this.apiUrl, data);
  }

  update(id: string, data: Partial<Conceptoexpensa>): Observable<Conceptoexpensa> {
    return this.http.put<Conceptoexpensa>(`${this.apiUrl}/${id}`, data);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  deleteMultiple(ids: (string | number)[]): Observable<void> {
    return this.http.request<void>('delete', `${this.apiUrl}/bulk`, { body: ids });
  }
}
