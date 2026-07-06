import { Directive, ElementRef, HostListener, inject } from '@angular/core';

/**
 * Trims leading/trailing whitespace from an input when it loses focus, keeping the form
 * control value in sync. Prevents accidental spaces from polluting stored data.
 *
 * Usage: `<input appTrimInput formControlName="name" />`
 */
@Directive({
  selector: '[appTrimInput]',
  standalone: true,
})
export class TrimInputDirective {
  private readonly el = inject(ElementRef<HTMLInputElement>);

  @HostListener('blur')
  onBlur(): void {
    const input = this.el.nativeElement;
    const trimmed = input.value.trim();
    if (trimmed !== input.value) {
      input.value = trimmed;
      input.dispatchEvent(new Event('input', { bubbles: true }));
    }
  }
}
