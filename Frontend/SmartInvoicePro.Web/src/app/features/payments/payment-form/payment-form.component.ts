import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { PaymentService } from '../../../core/services/payment.service';
import { InvoiceService } from '../../../core/services/invoice.service';
import { getApiErrorMessage } from '../../../core/utils/api-error';
import { InvoiceListItem } from '../../../core/models/invoice.model';
import { PAYMENT_METHODS } from '../../../core/models/payment.model';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { todayIso } from '../../../core/utils/format.utils';

@Component({
  selector: 'app-payment-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink, PageHeaderComponent],
  templateUrl: './payment-form.component.html',
  styleUrl: './payment-form.component.scss',
})
export class PaymentFormComponent {
  private readonly fb = inject(FormBuilder);
  private readonly paymentService = inject(PaymentService);
  private readonly invoiceService = inject(InvoiceService);
  private readonly router = inject(Router);

  readonly invoices = signal<InvoiceListItem[]>([]);
  readonly loading = signal(false);
  readonly error = signal('');
  readonly methods = PAYMENT_METHODS;

  readonly form = this.fb.nonNullable.group({
    invoiceId: ['', Validators.required],
    paymentDate: [todayIso(), Validators.required],
    amountPaid: [0, [Validators.required, Validators.min(0.01)]],
    paymentMethod: ['Bank Transfer', Validators.required],
    referenceNumber: [''],
    notes: [''],
  });

  constructor() {
    this.invoiceService.getAll(1, 100).subscribe((r) => this.invoices.set(r.items.filter((i) => i.balanceAmount > 0)));
  }

  submit(): void {
    if (this.form.invalid) return;
    this.loading.set(true);
    this.paymentService.create(this.form.getRawValue()).subscribe({
      next: () => { this.loading.set(false); this.router.navigate(['/payments']); },
      error: (err) => { this.loading.set(false); this.error.set(getApiErrorMessage(err, 'Failed')); },
    });
  }
}
