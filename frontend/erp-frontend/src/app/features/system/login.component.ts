import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { switchMap } from 'rxjs';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { AuthService } from '@erp-platform/core';
import { BRANDING } from '../../branding';

/**
 * Real sign-in page. Posts credentials to `POST /api/auth/token` via {@link AuthService}, hydrates
 * the current user from `/api/auth/me`, then navigates to the `returnUrl` (or the dashboard).
 */
@Component({
  selector: 'app-login',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatProgressBarModule,
  ],
  template: `
    <div class="page" style="display:flex;justify-content:center;padding:3rem 1rem;">
      <mat-card style="width:100%;max-width:420px;padding:1.5rem;">
        <div style="text-align:center;margin-bottom:1rem;">
          <mat-icon style="font-size:2.5rem;width:2.5rem;height:2.5rem;">lock</mat-icon>
          <h1 style="margin:.5rem 0 0;font-size:1.5rem;">{{ brand.appName }}</h1>
          <p style="margin:.25rem 0 0;color:rgba(0,0,0,.6);">{{ brand.description }}</p>
        </div>

        @if (loading()) {
          <mat-progress-bar mode="indeterminate"></mat-progress-bar>
        }

        <form [formGroup]="form" (ngSubmit)="submit()" style="display:flex;flex-direction:column;gap:.5rem;margin-top:1rem;">
          <mat-form-field appearance="outline">
            <mat-label>Username</mat-label>
            <input matInput formControlName="userName" autocomplete="username" />
          </mat-form-field>

          <mat-form-field appearance="outline">
            <mat-label>Password</mat-label>
            <input matInput type="password" formControlName="password" autocomplete="current-password" />
          </mat-form-field>

          @if (error()) {
            <p style="color:#b3261e;margin:.25rem 0;">{{ error() }}</p>
          }

          <button mat-raised-button color="primary" type="submit" [disabled]="form.invalid || loading()">
            Sign in
          </button>
        </form>
      </mat-card>
    </div>
  `,
})
export class LoginComponent {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  protected readonly brand = BRANDING;
  protected readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);

  protected readonly form = this.fb.nonNullable.group({
    userName: ['', Validators.required],
    password: ['', Validators.required],
  });

  protected submit(): void {
    if (this.form.invalid || this.loading()) {
      return;
    }
    this.loading.set(true);
    this.error.set(null);

    const { userName, password } = this.form.getRawValue();
    this.auth
      .login(userName, password)
      .pipe(switchMap(() => this.auth.loadCurrentUser()))
      .subscribe({
        next: () => {
          const returnUrl = this.route.snapshot.queryParamMap.get('returnUrl') ?? '/dashboard';
          void this.router.navigateByUrl(returnUrl);
        },
        error: () => {
          this.loading.set(false);
          this.error.set('Invalid username or password.');
        },
      });
  }
}
