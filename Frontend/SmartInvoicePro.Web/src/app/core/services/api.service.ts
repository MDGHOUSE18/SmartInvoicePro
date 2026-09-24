import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map, throwError } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api-response.model';

@Injectable({ providedIn: 'root' })
export class ApiService {
  private readonly http = inject(HttpClient);
  readonly baseUrl = environment.apiUrl;

  get<T>(path: string, params?: Record<string, string | number | boolean | undefined | null>): Observable<T> {
    return this.http
      .get<ApiResponse<T>>(`${this.baseUrl}/${path}`, { params: this.buildParams(params) })
      .pipe(map((res) => this.unwrap(res)));
  }

  post<T>(path: string, body?: unknown): Observable<T> {
    return this.http
      .post<ApiResponse<T>>(`${this.baseUrl}/${path}`, body ?? {})
      .pipe(map((res) => this.unwrap(res)));
  }

  put<T>(path: string, body?: unknown): Observable<T> {
    return this.http
      .put<ApiResponse<T>>(`${this.baseUrl}/${path}`, body ?? {})
      .pipe(map((res) => this.unwrap(res)));
  }

  delete<T = void>(path: string): Observable<T> {
    return this.http
      .delete<ApiResponse<T>>(`${this.baseUrl}/${path}`)
      .pipe(map((res) => this.unwrap(res)));
  }

  getBlob(path: string, params?: Record<string, string | number | boolean | undefined | null>): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/${path}`, {
      params: this.buildParams(params),
      responseType: 'blob',
    });
  }

  postEmpty(path: string): Observable<string | null> {
    return this.http.post<ApiResponse<unknown>>(`${this.baseUrl}/${path}`, {}).pipe(
      map((res) => {
        if (!res.success) {
          throw new Error(res.message ?? 'Request failed');
        }
        return res.message;
      })
    );
  }

  private unwrap<T>(res: ApiResponse<T>): T {
    if (!res.success) {
      const msg = res.message ?? res.errors?.join(', ') ?? 'Request failed';
      throw new Error(msg);
    }
    return res.data;
  }

  private buildParams(params?: Record<string, string | number | boolean | undefined | null>): HttpParams | undefined {
    if (!params) return undefined;
    let httpParams = new HttpParams();
    for (const [key, value] of Object.entries(params)) {
      if (value !== undefined && value !== null && value !== '') {
        httpParams = httpParams.set(key, String(value));
      }
    }
    return httpParams;
  }
}
