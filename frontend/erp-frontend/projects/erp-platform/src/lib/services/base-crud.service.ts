import { inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService, QueryParams } from './api.service';
import { PagedResponse, PaginationRequest } from '../models';

/**
 * Generic CRUD service for standard ERP modules. Extend it per module and provide the
 * resource path; the inherited methods cover list/get/create/update/delete against the
 * standard backend response envelopes.
 *
 * @typeParam TDto       Read model returned by the API.
 * @typeParam TCreateDto Payload for create requests.
 * @typeParam TUpdateDto Payload for update requests.
 * @typeParam TKey       Identifier type (defaults to string).
 */
export abstract class BaseCrudService<
  TDto,
  TCreateDto = Partial<TDto>,
  TUpdateDto = Partial<TDto>,
  TKey = string,
> {
  protected readonly api = inject(ApiService);

  /** Resource path relative to the API base, e.g. `users`. */
  protected abstract readonly resource: string;

  list(request: PaginationRequest, filters?: QueryParams): Observable<PagedResponse<TDto>> {
    return this.api.getPaged<TDto>(this.resource, request, filters);
  }

  getById(id: TKey): Observable<TDto | null> {
    return this.api.get<TDto>(`${this.resource}/${id}`);
  }

  create(payload: TCreateDto): Observable<TDto | null> {
    return this.api.post<TDto>(this.resource, payload);
  }

  update(id: TKey, payload: TUpdateDto): Observable<TDto | null> {
    return this.api.put<TDto>(`${this.resource}/${id}`, payload);
  }

  delete(id: TKey): Observable<unknown> {
    return this.api.delete(`${this.resource}/${id}`);
  }
}
