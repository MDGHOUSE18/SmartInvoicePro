import { Injectable, inject } from '@angular/core';
import { ApiService } from './api.service';
import { Payment, CreatePaymentRequest } from '../models/payment.model';
import { PagedResult } from '../models/api-response.model';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class PaymentService {
  private readonly api = inject(ApiService);

  getAll(page = 1, pageSize = 20, invoiceId?: string): Observable<PagedResult<Payment>> {
    return this.api.get<PagedResult<Payment>>('payments', { page, pageSize, invoiceId });
  }

  getById(id: string): Observable<Payment> {
    return this.api.get<Payment>(`payments/${id}`);
  }

  create(dto: CreatePaymentRequest): Observable<Payment> {
    return this.api.post<Payment>('payments', dto);
  }

  delete(id: string): Observable<void> {
    return this.api.delete(`payments/${id}`);
  }
}
