import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

/**
 * Reusable empty-result placeholder with an icon, message and optional call-to-action.
 */
@Component({
  selector: 'app-generic-empty-state',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [MatButtonModule, MatIconModule],
  templateUrl: './generic-empty-state.component.html',
  styleUrl: './generic-empty-state.component.scss',
})
export class GenericEmptyStateComponent {
  @Input() icon = 'inbox';
  @Input() message = 'No records found.';
  @Input() description?: string;
  @Input() actionLabel?: string;

  @Output() action = new EventEmitter<void>();
}
