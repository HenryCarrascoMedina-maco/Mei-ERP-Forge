import { Directive, ElementRef, HostListener, Input, inject } from '@angular/core';

/**
 * Restricts an input to numeric characters. Optionally allows a decimal separator and a
 * leading minus sign. Works with both template-driven and reactive forms because it
 * re-dispatches an `input` event after sanitizing so Angular's value accessor stays in sync.
 *
 * Usage: `<input appOnlyNumbers [allowDecimals]="true" formControlName="price" />`
 */
@Directive({
  selector: '[appOnlyNumbers]',
  standalone: true,
})
export class OnlyNumbersDirective {
  private readonly el = inject(ElementRef<HTMLInputElement>);

  @Input() allowDecimals = false;
  @Input() allowNegative = false;

  @HostListener('input')
  onInput(): void {
    const input = this.el.nativeElement;
    const original = input.value;
    const sanitized = this.sanitize(original);

    if (sanitized !== original) {
      input.value = sanitized;
      input.dispatchEvent(new Event('input', { bubbles: true }));
    }
  }

  private sanitize(value: string): string {
    let result = value.replace(/[^0-9.\-]/g, '');

    if (!this.allowDecimals) {
      result = result.replace(/\./g, '');
    } else {
      // Keep only the first decimal point.
      const firstDot = result.indexOf('.');
      if (firstDot !== -1) {
        result = result.slice(0, firstDot + 1) + result.slice(firstDot + 1).replace(/\./g, '');
      }
    }

    if (!this.allowNegative) {
      result = result.replace(/-/g, '');
    } else {
      // Allow a single leading minus only.
      const negative = result.startsWith('-');
      result = (negative ? '-' : '') + result.replace(/-/g, '');
    }

    return result;
  }
}
