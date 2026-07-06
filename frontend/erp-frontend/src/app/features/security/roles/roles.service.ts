import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService, BaseCrudService } from '@erp-platform/core';
import { SecurityRole } from './roles.model';

/** CRUD service for roles, plus the assignable-permission catalog. */
@Injectable({ providedIn: 'root' })
export class RolesService extends BaseCrudService<SecurityRole> {
  protected readonly resource = 'security/roles';
  private readonly apiService = inject(ApiService);

  /** The catalog of assignable permission keys (for the role editor). */
  availablePermissions(): Observable<string[] | null> {
    return this.apiService.get<string[]>('security/roles/permissions');
  }
}
