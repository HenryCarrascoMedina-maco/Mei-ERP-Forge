import { Pipe, PipeTransform } from '@angular/core';

const UNITS = ['B', 'KB', 'MB', 'GB', 'TB', 'PB'];

/**
 * Converts a byte count into a human-readable size.
 *
 * Usage: `{{ file.size | fileSize }}` → "1.4 MB" · `{{ bytes | fileSize:2 }}`
 */
@Pipe({ name: 'fileSize', standalone: true })
export class FileSizePipe implements PipeTransform {
  transform(bytes: number | null | undefined, decimals = 1): string {
    if (bytes === null || bytes === undefined || Number.isNaN(bytes)) {
      return '';
    }
    if (bytes <= 0) {
      return '0 B';
    }

    const exponent = Math.min(Math.floor(Math.log(bytes) / Math.log(1024)), UNITS.length - 1);
    const size = bytes / Math.pow(1024, exponent);
    return `${size.toFixed(exponent === 0 ? 0 : decimals)} ${UNITS[exponent]}`;
  }
}
