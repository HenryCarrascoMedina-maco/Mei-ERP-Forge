import { Injectable } from '@angular/core';
import { BaseCrudService } from '@erp-platform/core';
import { SecurityUser } from './users.model';

/** CRUD service for users (persisted identity store). */
@Injectable({ providedIn: 'root' })
export class UsersService extends BaseCrudService<SecurityUser> {
  protected readonly resource = 'security/users';
}
