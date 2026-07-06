/**
 * String helper functions: normalization, capitalization and slug generation.
 */
export class StringUtil {
  static isBlank(value: string | null | undefined): boolean {
    return value === null || value === undefined || value.trim() === '';
  }

  static capitalize(value: string): string {
    return value ? value.charAt(0).toUpperCase() + value.slice(1) : value;
  }

  static toTitleCase(value: string): string {
    return value.replace(/\b\w/g, (c) => c.toUpperCase());
  }

  /** Removes diacritics (accents). */
  static removeAccents(value: string): string {
    // Strip combining diacritical marks (U+0300–U+036F) after NFD normalization.
    return value.normalize('NFD').replace(new RegExp('[\\u0300-\\u036f]', 'g'), '');
  }

  /** URL-friendly slug: lowercase, no accents, hyphen-separated. */
  static slugify(value: string): string {
    return this.removeAccents(value)
      .toLowerCase()
      .trim()
      .replace(/[^a-z0-9]+/g, '-')
      .replace(/^-+|-+$/g, '');
  }

  static truncate(value: string, limit = 50, ellipsis = '…'): string {
    return value.length <= limit ? value : value.slice(0, limit).trimEnd() + ellipsis;
  }

  /** Initials from a name, e.g. "Ada Lovelace" → "AL". */
  static initials(value: string, max = 2): string {
    return value
      .split(/\s+/)
      .filter(Boolean)
      .slice(0, max)
      .map((w) => w.charAt(0).toUpperCase())
      .join('');
  }
}
