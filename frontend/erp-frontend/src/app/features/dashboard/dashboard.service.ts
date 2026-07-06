import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from '@erp-platform/core';
import { DashboardStats } from './dashboard.model';

/** Reads aggregated dashboard statistics from the backend (shared ApiResponse envelope). */
@Injectable({ providedIn: 'root' })
export class DashboardService {
  private readonly api = inject(ApiService);

  getStats(): Observable<DashboardStats | null> {
    return this.api.get<DashboardStats>('dashboard/stats');
  }
}
