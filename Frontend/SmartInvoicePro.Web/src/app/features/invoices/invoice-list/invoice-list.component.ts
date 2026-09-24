import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { InvoiceService } from '../../../core/services/invoice.service';
import { InvoiceListItem, INVOICE_STATUSES } from '../../../core/models/invoice.model';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { StatusBadgeComponent } from '../../../shared/components/status-badge/status-badge.component';
import { formatCurrency, formatDate } from '../../../core/utils/format.utils';

@Component({
  selector: 'app-invoice-list',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule, PageHeaderComponent, StatusBadgeComponent],
  templateUrl: './invoice-list.component.html',
  styleUrl: './invoice-list.component.scss',
})
export class InvoiceListComponent implements OnInit {
  private readonly invoiceService = inject(InvoiceService);
  readonly invoices = signal<InvoiceListItem[]>([]);
  readonly loading = signal(false);
  readonly page = signal(1);
  readonly totalPages = signal(1);
  search = '';
  status = '';
  readonly statuses = INVOICE_STATUSES;
  protected readonly formatCurrency = formatCurrency;
  protected readonly formatDate = formatDate;

  ngOnInit(): void { this.load(1); }

  load(p: number): void {
    this.loading.set(true);
    this.page.set(p);
    this.invoiceService.getAll(p, 20, this.status || undefined, undefined, this.search || undefined).subscribe({
      next: (r) => { this.invoices.set(r.items); this.totalPages.set(r.totalPages); this.loading.set(false); },
      error: () => this.loading.set(false),
    });
  }
}
