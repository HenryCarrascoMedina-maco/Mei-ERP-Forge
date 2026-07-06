import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { ERP_PLATFORM_CONFIG } from '../config/erp-platform.config';
import { ApiResponse, ImportResult } from '../models';
import { ApiService } from './api.service';
import { FileUtil } from '../utils/file.util';

/**
 * Handles import workflows: uploads a file to an import endpoint and returns the standard
 * {@link ImportResult}. Also maps row errors to readable strings for display.
 */
@Injectable({ providedIn: 'root' })
export class ImportService {
  private readonly http = inject(HttpClient);
  private readonly api = inject(ApiService);
  private readonly baseUrl = inject(ERP_PLATFORM_CONFIG).apiUrl;

  /** Uploads an import file and returns the parsed result. */
  import<T = unknown>(endpoint: string, file: File, fieldName = 'file'): Observable<ImportResult<T> | null> {
    const form = FileUtil.toFormData(file, fieldName);

    const clean = endpoint.startsWith('/') ? endpoint.slice(1) : endpoint;
    return this.http
      .post<ApiResponse<ImportResult<T>>>(`${this.baseUrl}/${clean}`, form)
      .pipe(map((res) => res.data ?? null));
  }

  /** Downloads an import template file from the backend. */
  downloadTemplate(endpoint: string, fileName: string): Observable<Blob> {
    return this.api.download(endpoint).pipe(
      map((blob) => {
        const url = URL.createObjectURL(blob);
        const anchor = document.createElement('a');
        anchor.href = url;
        anchor.download = fileName;
        anchor.click();
        URL.revokeObjectURL(url);
        return blob;
      }),
    );
  }

  /** Flattens an import result's row errors into readable messages. */
  toReadableErrors(result: ImportResult): string[] {
    return result.errors.map((e) =>
      e.column ? `Row ${e.row} · ${e.column}: ${e.message}` : `Row ${e.row}: ${e.message}`,
    );
  }
}
