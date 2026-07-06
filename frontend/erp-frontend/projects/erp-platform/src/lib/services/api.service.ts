import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpContext, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { ERP_PLATFORM_CONFIG } from '../config/erp-platform.config';
import { ApiResponse, PagedResponse, PaginationRequest } from '../models';

/** Loosely-typed bag of query parameters accepted by the API service. */
export type QueryParams = Record<string, string | number | boolean | null | undefined>;

export interface RequestOptions {
  params?: QueryParams;
  context?: HttpContext;
}

/**
 * Base HTTP service. Wraps {@link HttpClient}, prefixes the configured API URL and unwraps
 * the standard `ApiResponse<T>` / `PagedResponse<T>` envelopes so callers work with plain data.
 */
@Injectable({ providedIn: 'root' })
export class ApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(ERP_PLATFORM_CONFIG).apiUrl;

  /** GET that unwraps an `ApiResponse<T>` envelope. */
  get<T>(endpoint: string, options?: RequestOptions): Observable<T | null> {
    return this.http
      .get<ApiResponse<T>>(this.url(endpoint), this.httpOptions(options))
      .pipe(map((res) => res.data ?? null));
  }

  /** GET for paginated list endpoints returning the full `PagedResponse<T>`. */
  getPaged<T>(endpoint: string, request: PaginationRequest, extra?: QueryParams): Observable<PagedResponse<T>> {
    return this.http.get<PagedResponse<T>>(this.url(endpoint), {
      params: this.toHttpParams({ ...request, ...extra }),
    });
  }

  post<T>(endpoint: string, body: unknown, options?: RequestOptions): Observable<T | null> {
    return this.http
      .post<ApiResponse<T>>(this.url(endpoint), body, this.httpOptions(options))
      .pipe(map((res) => res.data ?? null));
  }

  put<T>(endpoint: string, body: unknown, options?: RequestOptions): Observable<T | null> {
    return this.http
      .put<ApiResponse<T>>(this.url(endpoint), body, this.httpOptions(options))
      .pipe(map((res) => res.data ?? null));
  }

  patch<T>(endpoint: string, body: unknown, options?: RequestOptions): Observable<T | null> {
    return this.http
      .patch<ApiResponse<T>>(this.url(endpoint), body, this.httpOptions(options))
      .pipe(map((res) => res.data ?? null));
  }

  delete<T>(endpoint: string, options?: RequestOptions): Observable<T | null> {
    return this.http
      .delete<ApiResponse<T>>(this.url(endpoint), this.httpOptions(options))
      .pipe(map((res) => res.data ?? null));
  }

  /** Raw download for files (Excel/PDF/CSV exports). */
  download(endpoint: string, params?: QueryParams): Observable<Blob> {
    return this.http.get(this.url(endpoint), {
      params: this.toHttpParams(params),
      responseType: 'blob',
    });
  }

  private url(endpoint: string): string {
    const clean = endpoint.startsWith('/') ? endpoint.slice(1) : endpoint;
    return `${this.baseUrl}/${clean}`;
  }

  private httpOptions(options?: RequestOptions) {
    return {
      params: this.toHttpParams(options?.params),
      context: options?.context,
    };
  }

  private toHttpParams(params?: QueryParams): HttpParams {
    let httpParams = new HttpParams();
    if (!params) {
      return httpParams;
    }
    for (const [key, value] of Object.entries(params)) {
      if (value !== null && value !== undefined && value !== '') {
        httpParams = httpParams.set(key, String(value));
      }
    }
    return httpParams;
  }
}
