import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpEventType } from '@angular/common/http';
import { Observable, filter, map } from 'rxjs';
import { ERP_PLATFORM_CONFIG } from '../config/erp-platform.config';
import { ApiResponse, FileMetadata, UploadProgress } from '../models';
import { ApiService } from './api.service';
import { ExportUtil } from '../utils/export.util';
import { FileUtil } from '../utils/file.util';

/** Either upload progress or the final metadata, emitted while uploading. */
export type UploadEvent =
  | { type: 'progress'; progress: UploadProgress }
  | { type: 'done'; file: FileMetadata | null };

/**
 * Handles file upload (multipart, with progress), download and deletion against the backend.
 */
@Injectable({ providedIn: 'root' })
export class FileService {
  private readonly http = inject(HttpClient);
  private readonly api = inject(ApiService);
  private readonly baseUrl = inject(ERP_PLATFORM_CONFIG).apiUrl;

  /** Uploads a single file, reporting progress and the resulting metadata. */
  upload(endpoint: string, file: File, fieldName = 'file'): Observable<UploadEvent> {
    const form = FileUtil.toFormData(file, fieldName);

    return this.http
      .post<ApiResponse<FileMetadata>>(this.url(endpoint), form, {
        reportProgress: true,
        observe: 'events',
      })
      .pipe(
        filter((event) => event.type === HttpEventType.UploadProgress || event.type === HttpEventType.Response),
        map((event): UploadEvent => {
          if (event.type === HttpEventType.UploadProgress) {
            const total = event.total ?? 0;
            return {
              type: 'progress',
              progress: {
                loaded: event.loaded,
                total,
                percent: total > 0 ? Math.round((event.loaded / total) * 100) : 0,
              },
            };
          }
          const body = (event as { body?: ApiResponse<FileMetadata> }).body;
          return { type: 'done', file: body?.data ?? null };
        }),
      );
  }

  /** Downloads a file by endpoint and saves it with the given name. */
  download(endpoint: string, fileName: string): Observable<void> {
    return this.api.download(endpoint).pipe(map((blob) => ExportUtil.downloadBlob(blob, fileName)));
  }

  /** Fetches metadata for a stored file. */
  getMetadata(endpoint: string): Observable<FileMetadata | null> {
    return this.api.get<FileMetadata>(endpoint);
  }

  /** Deletes a stored file. */
  delete(endpoint: string): Observable<unknown> {
    return this.api.delete(endpoint);
  }

  private url(endpoint: string): string {
    const clean = endpoint.startsWith('/') ? endpoint.slice(1) : endpoint;
    return `${this.baseUrl}/${clean}`;
  }
}
