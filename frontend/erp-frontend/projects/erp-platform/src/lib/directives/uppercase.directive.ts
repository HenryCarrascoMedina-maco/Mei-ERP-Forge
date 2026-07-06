import { Directive, ElementRef, HostListener, inject } from '@angular/core';

/**
 * Converts the text of an input to uppercase as the user types, keeping the form control in sync.
 *
 * Usage: `<input appUppercase formControlName="code" />`
 */
@Directive({
  selector: '[appUppercase]',
  standalone: true,
})
export class UppercaseDirective {
  private readonly el = inject(ElementRef<HTMLInputElement>);

  @HostListener('input')
  onInput(): void {
    const input = this.el.nativeElement;
    const upper = input.value.toUpperCase();
    if (upper !== input.value) {
      const start = input.selectionStart;
      const end = input.selectionEnd;
      input.value = upper;
      if (start !== null && end !== null) {
        input.setSelectionRange(start, end);
      }
      input.dispatchEvent(new Event('input', { bubbles: true }));
    }
  }
}
