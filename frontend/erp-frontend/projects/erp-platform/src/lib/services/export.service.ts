import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';
import { ApiService } from './api.service';
import { ExportUtil } from '../utils/export.util';

/** Supported export formats. */
export type ExportType = 'csv' | 'excel' | 'pdf';

const EXTENSIONS: Record<ExportType, string> = { csv: 'csv', excel: 'xlsx', pdf: 'pdf' };

/**
 * Centralizes data export. Server-side exports are downloaded as a Blob from an endpoint;
 * client-side CSV can be generated directly from in-memory rows.
 */
@Injectable({ providedIn: 'root' })
export class ExportService {
  private readonly api = inject(ApiService);

  /** Triggers a browser download for an existing Blob. */
  downloadBlob(blob: Blob, fileName: string): void {
    ExportUtil.downloadBlob(blob, fileName);
  }

  /** Generates a CSV from in-memory rows and downloads it (no backend round-trip). */
  exportLocalCsv<T extends Record<string, unknown>>(rows: T[], fileName: string, columns?: Array<keyof T>): void {
    ExportUtil.downloadCsv(rows, fileName, columns);
  }

  /**
   * Downloads a server-generated export. The backend endpoint should return the file bytes; the
   * `type` selects the file extension/content negotiation via the `format` query param.
   */
  exportFromApi(endpoint: string, fileNameBase: string, type: ExportType = 'csv'): Observable<void> {
    return this.api
      .download(endpoint, { format: type })
      .pipe(map((blob) => this.downloadBlob(blob, `${fileNameBase}.${EXTENSIONS[type]}`)));
  }
}
