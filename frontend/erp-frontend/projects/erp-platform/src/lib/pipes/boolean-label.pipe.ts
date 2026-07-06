import { Pipe, PipeTransform } from '@angular/core';

/**
 * Converts a boolean (or truthy/falsy value) into a readable label.
 *
 * Usage: `{{ isActive | booleanLabel }}` → "Yes"/"No"
 *        `{{ isActive | booleanLabel:'Active':'Inactive' }}`
 */
@Pipe({ name: 'booleanLabel', standalone: true })
export class BooleanLabelPipe implements PipeTransform {
  transform(value: unknown, trueLabel = 'Yes', falseLabel = 'No'): string {
    return value ? trueLabel : falseLabel;
  }
}
