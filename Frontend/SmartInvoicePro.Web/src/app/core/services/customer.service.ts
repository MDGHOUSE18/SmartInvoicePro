import { Injectable, inject } from '@angular/core';
import { ApiService } from './api.service';
import { Customer, CustomerListItem, CreateCustomerRequest, UpdateCustomerRequest } from '../models/customer.model';
import { PagedResult } from '../models/api-response.model';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class CustomerService {
  private readonly api = inject(ApiService);

  getAll(page = 1, pageSize = 20, search?: string): Observable<PagedResult<CustomerListItem>> {
    return this.api.get<PagedResult<CustomerListItem>>('customers', { page, pageSize, search });
  }

  getById(id: string): Observable<Customer> {
    return this.api.get<Customer>(`customers/${id}`);
  }

  create(dto: CreateCustomerRequest): Observable<Customer> {
    return this.api.post<Customer>('customers', dto);
  }

  update(id: string, dto: UpdateCustomerRequest): Observable<Customer> {
    return this.api.put<Customer>(`customers/${id}`, dto);
  }

  delete(id: string): Observable<void> {
    return this.api.delete(`customers/${id}`);
  }

  exportExcel(): Observable<Blob> {
    return this.api.getBlob('customers/export/excel');
  }
}
