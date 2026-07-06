/**
 * Client-side export helpers: trigger browser downloads and build simple CSV from data.
 * For server-generated files (Excel/PDF), download the `Blob` returned by `ApiService.download`.
 */
export class ExportUtil {
  /** Triggers a browser download for a Blob. */
  static downloadBlob(blob: Blob, fileName: string): void {
    const url = URL.createObjectURL(blob);
    const anchor = document.createElement('a');
    anchor.href = url;
    anchor.download = fileName;
    anchor.click();
    URL.revokeObjectURL(url);
  }

  /** Downloads arbitrary text content (CSV, JSON, ...) as a file. */
  static downloadText(content: string, fileName: string, mimeType = 'text/plain'): void {
    this.downloadBlob(new Blob([content], { type: `${mimeType};charset=utf-8;` }), fileName);
  }

  /** Serializes an array of objects to CSV. Columns default to the keys of the first row. */
  static toCsv<T extends Record<string, unknown>>(rows: T[], columns?: Array<keyof T>): string {
    if (rows.length === 0) {
      return '';
    }
    const keys = columns ?? (Object.keys(rows[0]) as Array<keyof T>);
    const escape = (value: unknown): string => {
      const text = value === null || value === undefined ? '' : String(value);
      return /[",\n]/.test(text) ? `"${text.replace(/"/g, '""')}"` : text;
    };

    const header = keys.map((k) => escape(String(k))).join(',');
    const body = rows.map((row) => keys.map((k) => escape(row[k])).join(',')).join('\n');
    return `${header}\n${body}`;
  }

  /** Builds a CSV from data and downloads it. */
  static downloadCsv<T extends Record<string, unknown>>(rows: T[], fileName: string, columns?: Array<keyof T>): void {
    this.downloadText(this.toCsv(rows, columns), fileName, 'text/csv');
  }

  /** Downloads any serializable value as a pretty-printed JSON file. */
  static downloadJson(data: unknown, fileName: string): void {
    this.downloadText(JSON.stringify(data, null, 2), fileName, 'application/json');
  }
}
