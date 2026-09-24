import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { PaymentService } from '../../../core/services/payment.service';
import { Payment } from '../../../core/models/payment.model';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { formatCurrency, formatDate } from '../../../core/utils/format.utils';

@Component({
  selector: 'app-payment-list',
  standalone: true,
  imports: [CommonModule, RouterLink, PageHeaderComponent],
  templateUrl: './payment-list.component.html',
  styleUrl: './payment-list.component.scss',
})
export class PaymentListComponent implements OnInit {
  private readonly paymentService = inject(PaymentService);
  readonly payments = signal<Payment[]>([]);
  readonly loading = signal(false);
  protected readonly formatCurrency = formatCurrency;
  protected readonly formatDate = formatDate;

  ngOnInit(): void {
    this.loading.set(true);
    this.paymentService.getAll(1, 50).subscribe({
      next: (r) => { this.payments.set(r.items); this.loading.set(false); },
      error: () => this.loading.set(false),
    });
  }
}
