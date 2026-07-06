/**
 * Array sorting, grouping and filtering helpers.
 */
export class ArrayUtil {
  /** Groups items by a key selector into a Map preserving insertion order. */
  static groupBy<T, K extends string | number>(items: T[], keyFn: (item: T) => K): Map<K, T[]> {
    const map = new Map<K, T[]>();
    for (const item of items) {
      const key = keyFn(item);
      const bucket = map.get(key);
      if (bucket) {
        bucket.push(item);
      } else {
        map.set(key, [item]);
      }
    }
    return map;
  }

  /** Stable sort by a comparable property. */
  static sortBy<T>(items: T[], keyFn: (item: T) => string | number, direction: 'asc' | 'desc' = 'asc'): T[] {
    const factor = direction === 'desc' ? -1 : 1;
    return [...items].sort((a, b) => {
      const av = keyFn(a);
      const bv = keyFn(b);
      if (av < bv) return -1 * factor;
      if (av > bv) return 1 * factor;
      return 0;
    });
  }

  static unique<T>(items: T[]): T[] {
    return [...new Set(items)];
  }

  static uniqueBy<T, K>(items: T[], keyFn: (item: T) => K): T[] {
    const seen = new Set<K>();
    const result: T[] = [];
    for (const item of items) {
      const key = keyFn(item);
      if (!seen.has(key)) {
        seen.add(key);
        result.push(item);
      }
    }
    return result;
  }

  static chunk<T>(items: T[], size: number): T[][] {
    if (size <= 0) return [items];
    const chunks: T[][] = [];
    for (let i = 0; i < items.length; i += size) {
      chunks.push(items.slice(i, i + size));
    }
    return chunks;
  }

  static toMap<T, K>(items: T[], keyFn: (item: T) => K): Map<K, T> {
    return new Map(items.map((item) => [keyFn(item), item]));
  }

  static sum<T>(items: T[], valueFn: (item: T) => number): number {
    return items.reduce((acc, item) => acc + valueFn(item), 0);
  }
}
