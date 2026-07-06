import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { PageChangeEvent } from '../generic-table/models/table-event.model';

/**
 * Reusable pagination control. Thin wrapper over Angular Material's paginator that emits a
 * normalized {@link PageChangeEvent} with a 1-based page index (matching the backend).
 */
@Component({
  selector: 'app-generic-pagination',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [MatPaginatorModule],
  templateUrl: './generic-pagination.component.html',
})
export class GenericPaginationComponent {
  /** 1-based current page. */
  @Input() page = 1;
  @Input() pageSize = 10;
  @Input() totalItems = 0;
  @Input() pageSizeOptions: number[] = [10, 25, 50, 100];

  @Output() pageChange = new EventEmitter<PageChangeEvent>();

  /** MatPaginator uses a 0-based index. */
  get pageIndex(): number {
    return Math.max(0, this.page - 1);
  }

  onPage(event: PageEvent): void {
    this.pageChange.emit({ page: event.pageIndex + 1, pageSize: event.pageSize });
  }
}
