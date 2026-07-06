import { ChangeDetectionStrategy, Component, DestroyRef, Input, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, NavigationEnd, Router, RouterModule } from '@angular/router';
import { filter } from 'rxjs';
import { MatIconModule } from '@angular/material/icon';
import { BreadcrumbItem } from './models/breadcrumb-item.model';

/**
 * Reusable breadcrumb navigation. Accepts an explicit list of items, or builds the trail
 * automatically from the activated route tree when `autoFromRoute` is enabled — each route that
 * declares `data.breadcrumb` (a string) contributes a node with its resolved URL.
 *
 * Usage:
 * ```html
 * <app-generic-breadcrumb [items]="[{ label: 'Home', route: '/' }, { label: 'Users' }]" />
 * <app-generic-breadcrumb [autoFromRoute]="true" />
 * ```
 */
@Component({
  selector: 'app-generic-breadcrumb',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [CommonModule, RouterModule, MatIconModule],
  templateUrl: './generic-breadcrumb.component.html',
  styleUrl: './generic-breadcrumb.component.scss',
})
export class GenericBreadcrumbComponent {
  private readonly router = inject(Router);
  private readonly activatedRoute = inject(ActivatedRoute);
  private readonly destroyRef = inject(DestroyRef);

  private readonly _items = signal<BreadcrumbItem[]>([]);
  private auto = false;

  /** Resolved breadcrumb nodes (read-only signal consumed by the template). */
  readonly crumbs = this._items.asReadonly();

  /** Explicit breadcrumb items. Ignored while `autoFromRoute` is true. */
  @Input()
  set items(value: BreadcrumbItem[]) {
    if (!this.auto) {
      this._items.set(value ?? []);
    }
  }

  /** Home node prepended when building from the route. */
  @Input() home: BreadcrumbItem | null = { label: 'Home', route: '/', icon: 'home' };

  @Input()
  set autoFromRoute(value: boolean) {
    this.auto = value;
    if (value) {
      this.router.events
        .pipe(filter((e) => e instanceof NavigationEnd), takeUntilDestroyed(this.destroyRef))
        .subscribe(() => this.buildFromRoute());
      this.buildFromRoute();
    }
  }

  private buildFromRoute(): void {
    const crumbs: BreadcrumbItem[] = this.home ? [{ ...this.home }] : [];
    let route = this.activatedRoute.root;
    let url = '';

    while (route.firstChild) {
      route = route.firstChild;
      const segment = route.snapshot.url.map((s) => s.path).join('/');
      if (segment) {
        url += `/${segment}`;
      }
      const label = route.snapshot.data['breadcrumb'] as string | undefined;
      if (label) {
        crumbs.push({ label, route: url || '/' });
      }
    }

    this._items.set(crumbs);
  }

  isLast(index: number): boolean {
    return index === this._items().length - 1;
  }
}
