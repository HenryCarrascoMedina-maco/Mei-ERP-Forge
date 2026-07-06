import { Directive, ElementRef, HostListener, inject } from '@angular/core';

/**
 * Converts the text of an input to lowercase as the user types, keeping the form control in sync.
 *
 * Usage: `<input appLowercase formControlName="email" />`
 */
@Directive({
  selector: '[appLowercase]',
  standalone: true,
})
export class LowercaseDirective {
  private readonly el = inject(ElementRef<HTMLInputElement>);

  @HostListener('input')
  onInput(): void {
    const input = this.el.nativeElement;
    const lower = input.value.toLowerCase();
    if (lower !== input.value) {
      const start = input.selectionStart;
      const end = input.selectionEnd;
      input.value = lower;
      if (start !== null && end !== null) {
        input.setSelectionRange(start, end);
      }
      input.dispatchEvent(new Event('input', { bubbles: true }));
    }
  }
}
