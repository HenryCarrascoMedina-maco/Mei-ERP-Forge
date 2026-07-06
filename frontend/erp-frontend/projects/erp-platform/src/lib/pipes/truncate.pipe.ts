import { Pipe, PipeTransform } from '@angular/core';

/**
 * Shortens long text to a maximum length, appending an ellipsis. Optionally breaks on a word
 * boundary so words are not cut mid-way.
 *
 * Usage: `{{ text | truncate }}` (50) · `{{ text | truncate:20 }}` · `{{ text | truncate:20:'…':true }}`
 */
@Pipe({ name: 'truncate', standalone: true })
export class TruncatePipe implements PipeTransform {
  transform(value: string | null | undefined, limit = 50, ellipsis = '…', wordBoundary = false): string {
    if (!value) {
      return '';
    }
    if (value.length <= limit) {
      return value;
    }

    let truncated = value.slice(0, limit);
    if (wordBoundary) {
      const lastSpace = truncated.lastIndexOf(' ');
      if (lastSpace > 0) {
        truncated = truncated.slice(0, lastSpace);
      }
    }
    return truncated.trimEnd() + ellipsis;
  }
}
