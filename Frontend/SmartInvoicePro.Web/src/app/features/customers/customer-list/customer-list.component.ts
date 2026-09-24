import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CustomerService } from '../../../core/services/customer.service';
import { CustomerListItem } from '../../../core/models/customer.model';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { StatusBadgeComponent } from '../../../shared/components/status-badge/status-badge.component';
import { downloadBlob } from '../../../core/utils/format.utils';
import { getApiErrorMessage } from '../../../core/utils/api-error';

@Component({
  selector: 'app-customer-list',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule, PageHeaderComponent, StatusBadgeComponent],
  templateUrl: './customer-list.component.html',
  styleUrl: './customer-list.component.scss',
})
export class CustomerListComponent implements OnInit {
  private readonly customerService = inject(CustomerService);

  readonly customers = signal<CustomerListItem[]>([]);
  readonly loading = signal(false);
  readonly page = signal(1);
  readonly totalPages = signal(1);
  search = '';
  sortKey = 'customerName';
  sortDir: 'asc' | 'desc' = 'asc';
  private searchTimeout?: ReturnType<typeof setTimeout>;

  ngOnInit(): void {
    this.load(1);
  }

  load(p: number): void {
    this.loading.set(true);
    this.page.set(p);
    this.customerService.getAll(p, 20, this.search || undefined).subscribe({
      next: (result) => {
        let items = [...result.items];
        items.sort((a, b) => {
          const av = String((a as unknown as Record<string, unknown>)[this.sortKey] ?? '');
          const bv = String((b as unknown as Record<string, unknown>)[this.sortKey] ?? '');
          return this.sortDir === 'asc' ? av.localeCompare(bv) : bv.localeCompare(av);
        });
        this.customers.set(items);
        this.totalPages.set(result.totalPages);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  onSearch(): void {
    clearTimeout(this.searchTimeout);
    this.searchTimeout = setTimeout(() => this.load(1), 300);
  }

  sort(key: string): void {
    if (this.sortKey === key) {
      this.sortDir = this.sortDir === 'asc' ? 'desc' : 'asc';
    } else {
      this.sortKey = key;
      this.sortDir = 'asc';
    }
    this.load(this.page());
  }

  sortIcon(key: string): string {
    return this.sortKey === key ? (this.sortDir === 'asc' ? '↑' : '↓') : '';
  }

  exportExcel(): void {
    this.customerService.exportExcel().subscribe({
      next: (blob) => downloadBlob(blob, 'customers.xlsx'),
      error: (err) => alert(getApiErrorMessage(err, 'Customer export failed. Please try again.')),
    });
  }
}
