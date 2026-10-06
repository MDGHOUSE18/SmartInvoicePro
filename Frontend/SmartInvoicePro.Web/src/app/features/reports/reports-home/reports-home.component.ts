import { Component, inject, signal } from '@angular/core';
import { Observable, catchError, forkJoin, of, tap } from 'rxjs';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { ReportService } from '../../../core/services/report.service';
import { SalesReport, TaxReport, CustomerReport, PaymentReport } from '../../../core/models/report.model';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { formatCurrency, formatDate, downloadBlob, todayIso } from '../../../core/utils/format.utils';
import { getApiErrorMessage } from '../../../core/utils/api-error';
import { SkeletonComponent } from '../../../shared/components/skeleton/skeleton.component';

@Component({
  selector: 'app-reports-home',
  standalone: true,
  imports: [SkeletonComponent, CommonModule, ReactiveFormsModule, PageHeaderComponent],
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
  readonly loading = signal(false);
  protected readonly formatCurrency = formatCurrency;
  protected readonly formatDate = formatDate;

  readonly filterForm = this.fb.nonNullable.group({
    fromDate: [this.firstOfMonth()],
    toDate: [todayIso()],
  });

  loadAll(): void {
    const filter = this.filterForm.getRawValue();
    this.error.set('');
    this.loading.set(true);
    forkJoin([
      this.settle(this.reportService.getSales(filter), (r) => this.sales.set(r), 'Failed to load sales report'),
      this.settle(this.reportService.getTax(filter), (r) => this.tax.set(r), 'Failed to load tax report'),
      this.settle(this.reportService.getCustomers(filter), (r) => this.customers.set(r), 'Failed to load customer report'),
      this.settle(this.reportService.getPayments(filter), (r) => this.payments.set(r), 'Failed to load payments report'),
    ]).subscribe(() => this.loading.set(false));
  }

  /** Applies one report's result; a failure shows its message without cancelling the others. */
  private settle<T>(source: Observable<T>, apply: (value: T) => void, failMessage: string): Observable<unknown> {
    return source.pipe(
      tap(apply),
      catchError((err) => {
        this.error.set(getApiErrorMessage(err, failMessage));
        return of(null);
      }),
    );
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
