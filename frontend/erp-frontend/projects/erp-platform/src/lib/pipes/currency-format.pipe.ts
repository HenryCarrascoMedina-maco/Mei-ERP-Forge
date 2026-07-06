import { Pipe, PipeTransform } from '@angular/core';

/**
 * Formats a number as currency using `Intl.NumberFormat`.
 *
 * Usage: `{{ amount | currencyFormat }}` · `{{ amount | currencyFormat:'EUR':'de-DE' }}`
 */
@Pipe({ name: 'currencyFormat', standalone: true })
export class CurrencyFormatPipe implements PipeTransform {
  transform(
    value: number | string | null | undefined,
    currency = 'USD',
    locale?: string,
    minimumFractionDigits = 2,
  ): string {
    if (value === null || value === undefined || value === '') {
      return '';
    }
    const amount = typeof value === 'string' ? Number(value) : value;
    if (Number.isNaN(amount)) {
      return '';
    }
    return new Intl.NumberFormat(locale, {
      style: 'currency',
      currency,
      minimumFractionDigits,
    }).format(amount);
  }
}
