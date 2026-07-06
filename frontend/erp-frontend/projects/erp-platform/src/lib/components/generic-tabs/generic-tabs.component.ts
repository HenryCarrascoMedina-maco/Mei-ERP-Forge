import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTabsModule } from '@angular/material/tabs';
import { MatIconModule } from '@angular/material/icon';
import { RouterModule } from '@angular/router';
import { PermissionService } from '../../services/permission.service';
import { TabItem } from './models/tab-item.model';

/** Selection / navigation strategy for the tabs. */
export type TabsMode = 'content' | 'route';

/**
 * Reusable tabs. Two modes:
 * - `content` (default): renders a Material tab bar and emits the active tab; the host projects
 *   the active panel and shows content based on the emitted key.
 * - `route`: renders a tab nav bar of router links; the host projects a `<router-outlet>`.
 *
 * Tabs are permission-aware (hidden when the user lacks `tab.permission`).
 *
 * Usage (content):
 * ```html
 * <app-generic-tabs [tabs]="tabs" [(selectedKey)]="active" (tabChange)="onTab($event)">
 *   @switch (active) { @case ('general') { ... } }
 * </app-generic-tabs>
 * ```
 */
@Component({
  selector: 'app-generic-tabs',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [CommonModule, MatTabsModule, MatIconModule, RouterModule],
  templateUrl: './generic-tabs.component.html',
  styleUrl: './generic-tabs.component.scss',
})
export class GenericTabsComponent {
  private readonly permissionService = inject(PermissionService);

  private readonly _tabs = signal<TabItem[]>([]);
  private readonly _selectedKey = signal<string | null>(null);

  @Input({ required: true })
  set tabs(value: TabItem[]) {
    this._tabs.set(value ?? []);
  }

  @Input() mode: TabsMode = 'content';

  @Input()
  set selectedKey(value: string | null) {
    this._selectedKey.set(value);
  }
  get selectedKey(): string | null {
    return this._selectedKey();
  }

  @Output() selectedKeyChange = new EventEmitter<string>();
  @Output() tabChange = new EventEmitter<TabItem>();

  /** Tabs the current user is allowed to see. */
  readonly visibleTabs = computed(() =>
    this._tabs().filter((tab) => this.permissionService.hasPermission(tab.permission)),
  );

  /** Active index within the visible tabs (content mode). */
  readonly selectedIndex = computed(() => {
    const key = this._selectedKey();
    const index = this.visibleTabs().findIndex((t) => t.key === key);
    return index === -1 ? 0 : index;
  });

  onIndexChange(index: number): void {
    const tab = this.visibleTabs()[index];
    if (!tab) {
      return;
    }
    this._selectedKey.set(tab.key);
    this.selectedKeyChange.emit(tab.key);
    this.tabChange.emit(tab);
  }
}
