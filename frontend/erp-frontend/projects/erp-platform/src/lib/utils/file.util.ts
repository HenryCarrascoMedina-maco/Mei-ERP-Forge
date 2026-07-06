const SIZE_UNITS = ['B', 'KB', 'MB', 'GB', 'TB'];

/**
 * File validation and filename helpers (no DOM side effects — see {@link ExportUtil} for downloads).
 */
export class FileUtil {
  /** Lowercase extension without the dot (e.g. "pdf"), or '' when none. */
  static getExtension(fileName: string): string {
    const dot = fileName.lastIndexOf('.');
    return dot === -1 ? '' : fileName.slice(dot + 1).toLowerCase();
  }

  static isAllowedExtension(fileName: string, allowed: string[]): boolean {
    const ext = this.getExtension(fileName);
    return allowed.map((e) => e.toLowerCase().replace(/^\./, '')).includes(ext);
  }

  static isWithinSize(sizeBytes: number, maxSizeMb: number): boolean {
    return sizeBytes <= maxSizeMb * 1024 * 1024;
  }

  /** Human-readable size string. */
  static formatSize(bytes: number, decimals = 1): string {
    if (bytes <= 0) return '0 B';
    const i = Math.min(Math.floor(Math.log(bytes) / Math.log(1024)), SIZE_UNITS.length - 1);
    return `${(bytes / Math.pow(1024, i)).toFixed(i === 0 ? 0 : decimals)} ${SIZE_UNITS[i]}`;
  }

  /** Strips unsafe characters from a filename. */
  static sanitizeName(fileName: string): string {
    return fileName.replace(/[\\/:*?"<>|]+/g, '_').trim();
  }

  /** Wraps a single file in a multipart `FormData` body under the given field name. */
  static toFormData(file: File, fieldName = 'file'): FormData {
    const form = new FormData();
    form.append(fieldName, file, file.name);
    return form;
  }
}
