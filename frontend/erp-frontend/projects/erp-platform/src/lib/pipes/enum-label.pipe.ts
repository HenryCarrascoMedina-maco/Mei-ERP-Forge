import { Pipe, PipeTransform } from '@angular/core';

/**
 * Converts an enum value into user-friendly text via a provided map; falls back to humanizing
 * the raw value (snake/kebab/PascalCase → spaced Title Case).
 *
 * Usage: `{{ status | enumLabel:OrderStatusLabels }}`
 */
@Pipe({ name: 'enumLabel', standalone: true })
export class EnumLabelPipe implements PipeTransform {
  transform(
    value: string | number | null | undefined,
    map?: Record<string | number, string>,
  ): string {
    if (value === null || value === undefined || value === '') {
      return '';
    }
    if (map && map[value] !== undefined) {
      return map[value];
    }
    return String(value)
      .replace(/([a-z])([A-Z])/g, '$1 $2')
      .replace(/[_-]+/g, ' ')
      .replace(/\b\w/g, (c) => c.toUpperCase());
  }
}
