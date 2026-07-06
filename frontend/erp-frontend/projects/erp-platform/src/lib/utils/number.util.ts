/**
 * Number formatting and math helpers.
 */
export class NumberUtil {
  static isNumber(value: unknown): value is number {
    return typeof value === 'number' && !Number.isNaN(value);
  }

  /** Rounds to a fixed number of decimals (returns a number, not a string). */
  static round(value: number, decimals = 2): number {
    const factor = Math.pow(10, decimals);
    return Math.round((value + Number.EPSILON) * factor) / factor;
  }

  static clamp(value: number, min: number, max: number): number {
    return Math.min(Math.max(value, min), max);
  }

  /** Safe percentage of `value` over `total` (0 when total is 0). */
  static percentage(value: number, total: number, decimals = 1): number {
    if (!total) return 0;
    return this.round((value / total) * 100, decimals);
  }

  /** Locale-aware number formatting. */
  static format(value: number, locale?: string, options?: Intl.NumberFormatOptions): string {
    return new Intl.NumberFormat(locale, options).format(value);
  }

  /** Coerces a value to a number, returning `fallback` when not parseable. */
  static toNumber(value: unknown, fallback = 0): number {
    const n = typeof value === 'number' ? value : Number(value);
    return Number.isNaN(n) ? fallback : n;
  }
}
