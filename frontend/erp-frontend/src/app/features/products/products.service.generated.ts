import { Injectable } from '@angular/core';
import { BaseCrudService } from '@erp-platform/core';
import { Product } from './products.model.generated';

/** Generated CRUD service for Product. DO NOT EDIT (extend in a separate file if needed). */
@Injectable({ providedIn: 'root' })
export class ProductService extends BaseCrudService<Product> {
  protected readonly resource = 'catalog/products';
}
