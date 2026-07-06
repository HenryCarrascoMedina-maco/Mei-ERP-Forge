import { ChangeDetectionStrategy, Component, OnInit, inject, signal } from '@angular/core';

import { GenericPageHeaderComponent } from '@erp-platform/core';
import { GenericKpiCardComponent } from '@erp-platform/core';
import { GenericCardComponent } from '@erp-platform/core';
import { GenericSkeletonComponent } from '@erp-platform/core';
import { GenericStatusBadgeComponent } from '@erp-platform/core';
import { BreadcrumbItem } from '@erp-platform/core';
import { DateFormatPipe } from '@erp-platform/core';
import { DashboardService } from './dashboard.service';
import { DashboardStats } from './dashboard.model';

/**
 * Dashboard module. Composed only from reusable components: page-header (+ breadcrumb), KPI cards,
 * summary cards and a recent-records list, with a skeleton placeholder during loading. Responsive
 * via CSS grids.
 */
@Component({
  selector: 'app-dashboard',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    GenericPageHeaderComponent,
    GenericKpiCardComponent,
    GenericCardComponent,
    GenericSkeletonComponent,
    GenericStatusBadgeComponent,
    DateFormatPipe,
  ],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss',
})
export class DashboardComponent implements OnInit {
  private readonly dashboardService = inject(DashboardService);

  readonly stats = signal<DashboardStats | null>(null);
  readonly loading = signal(true);

  readonly breadcrumbs: BreadcrumbItem[] = [
    { label: 'Home', route: '/', icon: 'home' },
    { label: 'Dashboard' },
  ];

  private readonly kpiIcons: Record<string, string> = {
    users: 'group',
    active: 'check_circle',
    categories: 'list_alt',
    pending: 'schedule',
  };

  ngOnInit(): void {
    this.dashboardService.getStats().subscribe({
      next: (stats) => {
        this.stats.set(stats);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  iconFor(key: string): string {
    return this.kpiIcons[key] ?? 'insights';
  }
}
