import { ChangeDetectionStrategy, Component } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';

/** Placeholder 403 page — the redirect target for the permission/role guards. */
@Component({
  selector: 'app-forbidden',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [MatCardModule, MatIconModule],
  template: `
    <div class="page">
      <mat-card style="max-width: 420px; margin: 3rem auto; padding: 1.5rem; text-align: center;">
        <mat-icon color="warn" style="font-size:2.5rem;width:2.5rem;height:2.5rem;">block</mat-icon>
        <h2>Access denied</h2>
        <p>You do not have permission to view this page.</p>
      </mat-card>
    </div>
  `,
})
export class ForbiddenComponent {}
