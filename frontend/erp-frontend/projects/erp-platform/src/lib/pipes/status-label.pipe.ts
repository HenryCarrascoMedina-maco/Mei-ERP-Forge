import { Pipe, PipeTransform } from '@angular/core';
import { DEFAULT_STATUS_MAP } from '../components/generic-status-badge/models/status-badge.model';

/**
 * Converts a status code into a readable label. Looks up the shared status map first, then any
 * custom map provided, and finally humanizes the raw code (snake/kebab → Title Case).
 *
 * Usage: `{{ 'pending' | statusLabel }}` → "Pending"
 *        `{{ code | statusLabel:{ ok: 'Operational' } }}`
 */
@Pipe({ name: 'statusLabel', standalone: true })
export class StatusLabelPipe implements PipeTransform {
  transform(value: string | null | undefined, customMap?: Record<string, string>): string {
    if (!value) {
      return '';
    }
    const key = value.trim().toLowerCase();

    if (customMap && customMap[key]) {
      return customMap[key];
    }
    if (DEFAULT_STATUS_MAP[key]) {
      return DEFAULT_STATUS_MAP[key].label;
    }
    return value
      .replace(/[_-]+/g, ' ')
      .replace(/\b\w/g, (c) => c.toUpperCase());
  }
}
