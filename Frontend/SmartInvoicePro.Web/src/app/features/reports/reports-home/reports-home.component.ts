import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { ReportService } from '../../../core/services/report.service';
import { SalesReport, TaxReport, CustomerReport, PaymentReport } from '../../../core/models/report.model';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { formatCurrency, formatDate, downloadBlob, todayIso } from '../../../core/utils/format.utils';
import { getApiErrorMessage } from '../../../core/utils/api-error';

@Component({
  selector: 'app-reports-home',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, PageHeaderComponent],
  templateUrl: './reports-home.component.html',
  styleUrl: './reports-home.component.scss',
})
export class ReportsHomeComponent {
  private readonly fb = inject(FormBuilder);
  private readonly reportService = inject(ReportService);

  readonly tab = signal<'sales' | 'tax' | 'customers' | 'payments'>('sales');
  readonly sales = signal<SalesReport | null>(null);
  readonly tax = signal<TaxReport | null>(null);
  readonly customers = signal<CustomerReport[]>([]);
  readonly payments = signal<PaymentReport | null>(null);
  readonly error = signal('');
  protected readonly formatCurrency = formatCurrency;
  protected readonly formatDate = formatDate;

  readonly filterForm = this.fb.nonNullable.group({
    fromDate: [this.firstOfMonth()],
    toDate: [todayIso()],
  });

  loadAll(): void {
    const filter = this.filterForm.getRawValue();
    this.error.set('');
    this.reportService.getSales(filter).subscribe({
      next: (r) => this.sales.set(r),
      error: (err) => this.error.set(getApiErrorMessage(err, 'Failed to load sales report')),
    });
    this.reportService.getTax(filter).subscribe({
      next: (r) => this.tax.set(r),
      error: (err) => this.error.set(getApiErrorMessage(err, 'Failed to load tax report')),
    });
    this.reportService.getCustomers(filter).subscribe({
      next: (r) => this.customers.set(r),
      error: (err) => this.error.set(getApiErrorMessage(err, 'Failed to load customer report')),
    });
    this.reportService.getPayments(filter).subscribe({
      next: (r) => this.payments.set(r),
      error: (err) => this.error.set(getApiErrorMessage(err, 'Failed to load payments report')),
    });
  }

  exportSales(): void {
    this.error.set('');
    this.reportService.exportSalesExcel(this.filterForm.getRawValue()).subscribe({
      next: (blob) => downloadBlob(blob, 'sales-report.xlsx'),
      error: (err) => this.error.set(getApiErrorMessage(err, 'Sales export failed')),
    });
  }

  exportTax(): void {
    this.error.set('');
    this.reportService.exportTaxExcel(this.filterForm.getRawValue()).subscribe({
      next: (blob) => downloadBlob(blob, 'tax-report.xlsx'),
      error: (err) => this.error.set(getApiErrorMessage(err, 'Tax export failed')),
    });
  }

  private firstOfMonth(): string {
    const d = new Date();
    return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-01`;
  }
}
