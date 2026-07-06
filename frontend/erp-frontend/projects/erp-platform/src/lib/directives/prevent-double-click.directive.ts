import { Directive, ElementRef, HostListener, Input, inject } from '@angular/core';

/**
 * Prevents duplicate submissions caused by rapid double-clicks. The first click is allowed to
 * propagate; subsequent clicks within {@link throttleTime} ms are swallowed while the host is
 * temporarily locked (disabled for native buttons, pointer-events disabled otherwise).
 *
 * Usage: `<button appPreventDoubleClick (click)="save()">Save</button>`
 */
@Directive({
  selector: '[appPreventDoubleClick]',
  standalone: true,
})
export class PreventDoubleClickDirective {
  private readonly el = inject(ElementRef<HTMLElement>);

  /** Lock window in milliseconds. */
  @Input() throttleTime = 1000;

  private locked = false;

  @HostListener('click', ['$event'])
  onClick(event: Event): void {
    if (this.locked) {
      event.preventDefault();
      event.stopImmediatePropagation();
      return;
    }

    this.lock();
    setTimeout(() => this.unlock(), this.throttleTime);
  }

  private lock(): void {
    this.locked = true;
    const node = this.el.nativeElement;
    if (node instanceof HTMLButtonElement) {
      node.disabled = true;
    } else {
      node.style.pointerEvents = 'none';
    }
  }

  private unlock(): void {
    this.locked = false;
    const node = this.el.nativeElement;
    if (node instanceof HTMLButtonElement) {
      node.disabled = false;
    } else {
      node.style.pointerEvents = '';
    }
  }
}
