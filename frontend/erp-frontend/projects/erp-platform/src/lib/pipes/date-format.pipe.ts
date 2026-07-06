import { Pipe, PipeTransform } from '@angular/core';

export type DateFormatMode = 'date' | 'datetime' | 'time';

type DateInput = Date | string | number | null | undefined;

/**
 * Formats a date consistently across the app using the browser locale.
 *
 * Usage: `{{ value | dateFormat }}` · `{{ value | dateFormat:'datetime' }}` · `{{ value | dateFormat:'time' }}`
 */
@Pipe({ name: 'dateFormat', standalone: true })
export class DateFormatPipe implements PipeTransform {
  transform(value: DateInput, mode: DateFormatMode = 'date', locale?: string): string {
    if (value === null || value === undefined || value === '') {
      return '';
    }
    const date = value instanceof Date ? value : new Date(value);
    if (Number.isNaN(date.getTime())) {
      return '';
    }

    const options: Intl.DateTimeFormatOptions =
      mode === 'datetime'
        ? { year: 'numeric', month: 'short', day: '2-digit', hour: '2-digit', minute: '2-digit' }
        : mode === 'time'
          ? { hour: '2-digit', minute: '2-digit' }
          : { year: 'numeric', month: 'short', day: '2-digit' };

    return new Intl.DateTimeFormat(locale, options).format(date);
  }
}
