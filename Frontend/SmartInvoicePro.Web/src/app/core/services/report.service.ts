import { Injectable, inject } from '@angular/core';
import { ApiService } from './api.service';
import {
  ReportFilter,
  SalesReport,
  TaxReport,
  CustomerReport,
  PaymentReport,
} from '../models/report.model';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class ReportService {
  private readonly api = inject(ApiService);

  getSales(filter: ReportFilter): Observable<SalesReport> {
    return this.api.get<SalesReport>('reports/sales', filter as Record<string, string>);
  }

  getTax(filter: ReportFilter): Observable<TaxReport> {
    return this.api.get<TaxReport>('reports/tax', filter as Record<string, string>);
  }

  getCustomers(filter: ReportFilter): Observable<CustomerReport[]> {
    return this.api.get<CustomerReport[]>('reports/customers', filter as Record<string, string>);
  }

  getPayments(filter: ReportFilter): Observable<PaymentReport> {
    return this.api.get<PaymentReport>('reports/payments', filter as Record<string, string>);
  }

  exportSalesExcel(filter: ReportFilter): Observable<Blob> {
    return this.api.getBlob('reports/sales/export', filter as Record<string, string>);
  }

  exportTaxExcel(filter: ReportFilter): Observable<Blob> {
    return this.api.getBlob('reports/tax/export', filter as Record<string, string>);
  }
}
