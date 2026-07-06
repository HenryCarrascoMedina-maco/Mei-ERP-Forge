/**
 * Route / URL helper functions.
 */
export class RouteUtil {
  /** Joins path segments with single slashes, trimming duplicates. */
  static join(...segments: Array<string | number>): string {
    return (
      '/' +
      segments
        .map((s) => String(s).replace(/^\/+|\/+$/g, ''))
        .filter((s) => s.length > 0)
        .join('/')
    );
  }

  /** Appends a query string from an object, skipping null/undefined/empty values. */
  static withQuery(path: string, params: Record<string, string | number | boolean | null | undefined>): string {
    const query = Object.entries(params)
      .filter(([, v]) => v !== null && v !== undefined && v !== '')
      .map(([k, v]) => `${encodeURIComponent(k)}=${encodeURIComponent(String(v))}`)
      .join('&');
    return query ? `${path}?${query}` : path;
  }

  /** Parses a query string (or `location.search`) into a plain object. */
  static parseQuery(search: string): Record<string, string> {
    const params = new URLSearchParams(search.startsWith('?') ? search.slice(1) : search);
    const result: Record<string, string> = {};
    params.forEach((value, key) => {
      result[key] = value;
    });
    return result;
  }
}
