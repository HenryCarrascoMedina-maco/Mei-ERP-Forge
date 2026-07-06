import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogModule } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';

export type ModalSize = 'sm' | 'md' | 'lg' | 'xl';

/**
 * Reusable modal wrapper. Provides a consistent header (title + close), a projected content
 * area and an optional footer. Designed to be rendered inside a {@link MatDialog} host.
 *
 * Usage:
 * ```html
 * <app-generic-modal [title]="'Edit user'" (closed)="ref.close()">
 *   <app-generic-form ...></app-generic-form>
 *   <ng-container modal-footer>
 *     <button mat-button (click)="ref.close()">Cancel</button>
 *   </ng-container>
 * </app-generic-modal>
 * ```
 */
@Component({
  selector: 'app-generic-modal',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [MatButtonModule, MatDialogModule, MatIconModule],
  templateUrl: './generic-modal.component.html',
  styleUrl: './generic-modal.component.scss',
})
export class GenericModalComponent {
  @Input() title = '';
  @Input() size: ModalSize = 'md';
  @Input() showClose = true;
  /** Hide the footer region entirely (e.g. read-only views). */
  @Input() showFooter = true;

  @Output() closed = new EventEmitter<void>();
}
