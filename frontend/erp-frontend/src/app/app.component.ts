import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { LoadingService, AuthService } from '@erp-platform/core';
import { APP_MENU } from './app.menu';
import { BRANDING } from './branding';

@Component({
  selector: 'app-root',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    RouterOutlet,
    RouterLink,
    RouterLinkActive,
    MatProgressBarModule,
    MatToolbarModule,
    MatButtonModule,
    MatIconModule,
    MatMenuModule,
  ],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss',
})
export class AppComponent {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  protected readonly loading = inject(LoadingService).isLoading;
  protected readonly title = BRANDING.appName;
  protected readonly nav = APP_MENU;

  constructor() {
    // Keep the browser tab in sync with the brand (overrides the static index.html title).
    inject(Title).setTitle(BRANDING.appName);
  }

  protected readonly isAuthenticated = this.auth.isAuthenticated;
  protected readonly currentUser = this.auth.currentUser;

  protected logout(): void {
    this.auth.logout().subscribe(() => this.router.navigate(['/login']));
  }
}
