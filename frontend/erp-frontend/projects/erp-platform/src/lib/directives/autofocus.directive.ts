import { AfterViewInit, Directive, ElementRef, Input, inject } from '@angular/core';

/**
 * Automatically focuses the host element after the view initializes. Bind a falsy value to
 * disable it conditionally.
 *
 * Usage: `<input appAutofocus />` or `<input [appAutofocus]="isFirstField" />`
 */
@Directive({
  selector: '[appAutofocus]',
  standalone: true,
})
export class AutofocusDirective implements AfterViewInit {
  private readonly el = inject(ElementRef<HTMLElement>);

  /** Set to false to skip auto-focusing. Defaults to true (bare attribute usage). */
  @Input() appAutofocus: boolean | '' = true;

  ngAfterViewInit(): void {
    const enabled = this.appAutofocus === '' || this.appAutofocus === true;
    if (enabled) {
      // Defer to the next macrotask so the element is fully rendered (e.g. inside a dialog).
      setTimeout(() => this.el.nativeElement.focus());
    }
  }
}
