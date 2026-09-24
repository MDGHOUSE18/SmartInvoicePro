import { Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { CustomerService } from '../../../core/services/customer.service';
import { Customer } from '../../../core/models/customer.model';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { StatusBadgeComponent } from '../../../shared/components/status-badge/status-badge.component';
import { formatDate } from '../../../core/utils/format.utils';

@Component({
  selector: 'app-customer-detail',
  standalone: true,
  imports: [CommonModule, RouterLink, PageHeaderComponent, StatusBadgeComponent],
  templateUrl: './customer-detail.component.html',
  styleUrl: './customer-detail.component.scss',
})
export class CustomerDetailComponent implements OnInit {
  private readonly customerService = inject(CustomerService);
  private readonly route = inject(ActivatedRoute);

  readonly customer = signal<Customer | null>(null);
  readonly loading = signal(true);
  protected readonly formatDate = formatDate;

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id')!;
    this.customerService.getById(id).subscribe({
      next: (c) => { this.customer.set(c); this.loading.set(false); },
      error: () => this.loading.set(false),
    });
  }
}
