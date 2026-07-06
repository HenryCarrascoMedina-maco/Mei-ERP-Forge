import { Injectable } from '@angular/core';
import { BaseCrudService } from '@erp-platform/core';
import { Customer } from './customers.model.generated';

/** Generated CRUD service for Customer. DO NOT EDIT (extend in a separate file if needed). */
@Injectable({ providedIn: 'root' })
export class CustomerService extends BaseCrudService<Customer> {
  protected readonly resource = 'sales/customers';
}
