import { Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { InvoiceService } from '../../../core/services/invoice.service';
import { Invoice } from '../../../core/models/invoice.model';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { StatusBadgeComponent } from '../../../shared/components/status-badge/status-badge.component';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog/confirm-dialog.component';
import { formatCurrency, formatDate, downloadBlob } from '../../../core/utils/format.utils';
import { formatItemTaxRate, invoiceTaxLabel } from '../../../core/utils/invoice.utils';
import { getApiErrorMessage } from '../../../core/utils/api-error';

@Component({
  selector: 'app-invoice-detail',
  standalone: true,
  imports: [CommonModule, RouterLink, PageHeaderComponent, StatusBadgeComponent, ConfirmDialogComponent],
  templateUrl: './invoice-detail.component.html',
  styleUrl: './invoice-detail.component.scss',
})
export class InvoiceDetailComponent implements OnInit {
  private readonly invoiceService = inject(InvoiceService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  readonly invoice = signal<Invoice | null>(null);
  readonly loading = signal(true);
  readonly message = signal('');
  readonly error = signal('');
  readonly markPaidOpen = signal(false);
  protected readonly formatCurrency = formatCurrency;
  protected readonly formatDate = formatDate;
  protected readonly formatItemTaxRate = formatItemTaxRate;
  protected readonly taxLabel = invoiceTaxLabel;

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id')!;
    this.invoiceService.getById(id).subscribe({
      next: (inv) => { this.invoice.set(inv); this.loading.set(false); },
      error: (err) => {
        this.loading.set(false);
        this.error.set(getApiErrorMessage(err, 'Failed to load invoice'));
      },
    });
  }

  downloadPdf(): void {
    const id = this.invoice()!.invoiceId;
    this.error.set('');
    this.invoiceService.downloadPdf(id).subscribe({
      next: (blob) => downloadBlob(blob, `invoice-${this.invoice()!.invoiceNumber}.pdf`),
      error: (err) => this.error.set(getApiErrorMessage(err, 'PDF download failed')),
    });
  }

  sendEmail(): void {
    this.error.set('');
    this.invoiceService.sendEmail(this.invoice()!.invoiceId).subscribe({
      next: (msg) => this.message.set(msg ?? 'Email logged to EmailLogs (demo)'),
      error: (err) => this.error.set(getApiErrorMessage(err, 'Send email failed')),
    });
  }

  clone(): void {
    this.error.set('');
    this.invoiceService.clone(this.invoice()!.invoiceId).subscribe({
      next: (inv) => this.router.navigate(['/invoices', inv.invoiceId]),
      error: (err) => this.error.set(getApiErrorMessage(err, 'Clone failed')),
    });
  }

  markPaid(): void {
    this.error.set('');
    this.invoiceService.markPaid(this.invoice()!.invoiceId).subscribe({
      next: (inv) => {
        this.invoice.set(inv);
        this.markPaidOpen.set(false);
        this.message.set('Invoice marked as paid');
      },
      error: (err) => this.error.set(getApiErrorMessage(err, 'Mark paid failed')),
    });
  }
}
