import { Injectable, inject } from '@angular/core';
import { ApiService } from './api.service';
import {
  Invoice,
  InvoiceListItem,
  InvoiceSummary,
  CreateInvoiceRequest,
  UpdateInvoiceRequest,
} from '../models/invoice.model';
import { PagedResult } from '../models/api-response.model';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class InvoiceService {
  private readonly api = inject(ApiService);

  getAll(
    page = 1,
    pageSize = 20,
    status?: string,
    customerId?: string,
    search?: string
  ): Observable<PagedResult<InvoiceListItem>> {
    return this.api.get<PagedResult<InvoiceListItem>>('invoices', { page, pageSize, status, customerId, search });
  }

  getSummary(): Observable<InvoiceSummary> {
    return this.api.get<InvoiceSummary>('invoices/summary');
  }

  getById(id: string): Observable<Invoice> {
    return this.api.get<Invoice>(`invoices/${id}`);
  }

  create(dto: CreateInvoiceRequest): Observable<Invoice> {
    return this.api.post<Invoice>('invoices', dto);
  }

  update(id: string, dto: UpdateInvoiceRequest): Observable<Invoice> {
    return this.api.put<Invoice>(`invoices/${id}`, dto);
  }

  delete(id: string): Observable<void> {
    return this.api.delete(`invoices/${id}`);
  }

  clone(id: string): Observable<Invoice> {
    return this.api.post<Invoice>(`invoices/${id}/clone`);
  }

  markPaid(id: string): Observable<Invoice> {
    return this.api.post<Invoice>(`invoices/${id}/mark-paid`);
  }

  sendEmail(id: string): Observable<string | null> {
    return this.api.postEmpty(`invoices/${id}/send`);
  }

  downloadPdf(id: string): Observable<Blob> {
    return this.api.getBlob(`invoices/${id}/pdf`);
  }
}
