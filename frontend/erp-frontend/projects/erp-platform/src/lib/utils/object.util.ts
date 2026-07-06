/**
 * Object cloning, comparison and cleanup helpers.
 */
export class ObjectUtil {
  /** Structured deep clone with a JSON fallback for older runtimes. */
  static deepClone<T>(value: T): T {
    if (typeof structuredClone === 'function') {
      return structuredClone(value);
    }
    return JSON.parse(JSON.stringify(value)) as T;
  }

  static isEmpty(value: unknown): boolean {
    if (value === null || value === undefined) return true;
    if (typeof value === 'string') return value.trim() === '';
    if (Array.isArray(value)) return value.length === 0;
    if (typeof value === 'object') return Object.keys(value as object).length === 0;
    return false;
  }

  /** Returns a copy without null/undefined (and optionally empty-string) values. */
  static removeNullish<T extends Record<string, unknown>>(obj: T, removeEmptyStrings = false): Partial<T> {
    const result: Partial<T> = {};
    for (const [key, value] of Object.entries(obj)) {
      const drop = value === null || value === undefined || (removeEmptyStrings && value === '');
      if (!drop) {
        result[key as keyof T] = value as T[keyof T];
      }
    }
    return result;
  }

  static pick<T extends object, K extends keyof T>(obj: T, keys: K[]): Pick<T, K> {
    const result = {} as Pick<T, K>;
    for (const key of keys) {
      if (key in obj) result[key] = obj[key];
    }
    return result;
  }

  static omit<T extends object, K extends keyof T>(obj: T, keys: K[]): Omit<T, K> {
    const result = { ...obj };
    for (const key of keys) {
      delete result[key];
    }
    return result;
  }

  /** Shallow-ish deep equality via JSON serialization (suitable for plain data objects). */
  static deepEqual(a: unknown, b: unknown): boolean {
    if (a === b) return true;
    return JSON.stringify(a) === JSON.stringify(b);
  }
}
