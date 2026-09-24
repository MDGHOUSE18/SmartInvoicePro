import { Component, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { CustomerService } from '../../../core/services/customer.service';
import { getApiErrorMessage } from '../../../core/utils/api-error';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';

@Component({
  selector: 'app-customer-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink, PageHeaderComponent],
  templateUrl: './customer-form.component.html',
  styleUrl: './customer-form.component.scss',
})
export class CustomerFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly customerService = inject(CustomerService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  readonly loading = signal(false);
  readonly error = signal('');
  isEdit = false;
  customerId = '';

  readonly form = this.fb.nonNullable.group({
    customerName: ['', Validators.required],
    companyName: [''],
    email: ['', [Validators.required, Validators.email]],
    phone: [''],
    address: [''],
    city: [''],
    state: [''],
    country: ['India'],
    taxNumber: [''],
    isActive: [true],
  });

  ngOnInit(): void {
    this.customerId = this.route.snapshot.paramMap.get('id') ?? '';
    this.isEdit = this.route.snapshot.url.some((s) => s.path === 'edit');
    if (this.isEdit && this.customerId) {
      this.customerService.getById(this.customerId).subscribe({
        next: (c) => this.form.patchValue(c),
        error: (err) => this.error.set(getApiErrorMessage(err)),
      });
    }
  }

  submit(): void {
    if (this.form.invalid) return;
    this.loading.set(true);
    const raw = this.form.getRawValue();
    const obs = this.isEdit
      ? this.customerService.update(this.customerId, raw)
      : this.customerService.create({
          customerName: raw.customerName,
          companyName: raw.companyName,
          email: raw.email,
          phone: raw.phone,
          address: raw.address,
          city: raw.city,
          state: raw.state,
          country: raw.country,
          taxNumber: raw.taxNumber,
        });
    obs.subscribe({
      next: (c) => {
        this.loading.set(false);
        this.router.navigate(['/customers', c.customerId]);
      },
      error: (err) => {
        this.loading.set(false);
        this.error.set(getApiErrorMessage(err, 'Save failed'));
      },
    });
  }
}
