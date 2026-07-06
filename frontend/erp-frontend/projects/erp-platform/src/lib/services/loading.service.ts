import { Injectable, computed, signal } from '@angular/core';

/**
 * Global loading-state manager. Uses a reference counter so overlapping HTTP requests
 * keep the indicator visible until the last one completes.
 */
@Injectable({ providedIn: 'root' })
export class LoadingService {
  private readonly activeRequests = signal(0);

  /** Reactive flag: true while at least one tracked request is in flight. */
  readonly isLoading = computed(() => this.activeRequests() > 0);

  show(): void {
    this.activeRequests.update((count) => count + 1);
  }

  hide(): void {
    this.activeRequests.update((count) => (count > 0 ? count - 1 : 0));
  }

  reset(): void {
    this.activeRequests.set(0);
  }
}
