type DateInput = Date | string | number | null | undefined;

/**
 * Date helper functions. Pure and side-effect free.
 */
export class DateUtil {
  static toDate(value: DateInput): Date | null {
    if (value === null || value === undefined || value === '') {
      return null;
    }
    const date = value instanceof Date ? value : new Date(value);
    return Number.isNaN(date.getTime()) ? null : date;
  }

  static isValid(value: DateInput): boolean {
    return this.toDate(value) !== null;
  }

  /** ISO string (date + time) or null. */
  static toIso(value: DateInput): string | null {
    return this.toDate(value)?.toISOString() ?? null;
  }

  static startOfDay(value: DateInput): Date | null {
    const date = this.toDate(value);
    if (!date) return null;
    const copy = new Date(date);
    copy.setHours(0, 0, 0, 0);
    return copy;
  }

  static endOfDay(value: DateInput): Date | null {
    const date = this.toDate(value);
    if (!date) return null;
    const copy = new Date(date);
    copy.setHours(23, 59, 59, 999);
    return copy;
  }

  static addDays(value: DateInput, days: number): Date | null {
    const date = this.toDate(value);
    if (!date) return null;
    const copy = new Date(date);
    copy.setDate(copy.getDate() + days);
    return copy;
  }

  /** Whole-day difference (b - a), or null if either is invalid. */
  static diffInDays(a: DateInput, b: DateInput): number | null {
    const start = this.startOfDay(a);
    const end = this.startOfDay(b);
    if (!start || !end) return null;
    return Math.round((end.getTime() - start.getTime()) / 86_400_000);
  }
}
