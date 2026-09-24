import { Component, inject, OnInit, signal } from '@angular/core';
import { FormArray, FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { InvoiceService } from '../../../core/services/invoice.service';
import { CustomerService } from '../../../core/services/customer.service';
import { ProductService } from '../../../core/services/product.service';
import { getApiErrorMessage } from '../../../core/utils/api-error';
import { CustomerListItem } from '../../../core/models/customer.model';
import { Product } from '../../../core/models/product.model';
import { INVOICE_STATUSES } from '../../../core/models/invoice.model';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { formatCurrency, todayIso } from '../../../core/utils/format.utils';

@Component({
  selector: 'app-invoice-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink, PageHeaderComponent],
  templateUrl: './invoice-form.component.html',
  styleUrl: './invoice-form.component.scss',
})
export class InvoiceFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly invoiceService = inject(InvoiceService);
  private readonly customerService = inject(CustomerService);
  private readonly productService = inject(ProductService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  readonly customers = signal<CustomerListItem[]>([]);
  readonly products = signal<Product[]>([]);
  readonly loading = signal(false);
  readonly error = signal('');
  isEdit = false;
  invoiceId = '';
  readonly statuses = INVOICE_STATUSES;

  readonly form = this.fb.nonNullable.group({
    customerId: ['', Validators.required],
    invoiceDate: [todayIso(), Validators.required],
    dueDate: [todayIso(), Validators.required],
    status: ['Draft'],
    notes: [''],
    discountAmount: [0],
    items: this.fb.array([this.createItemGroup()]),
  });

  get items(): FormArray {
    return this.form.get('items') as FormArray;
  }

  ngOnInit(): void {
    this.customerService.getAll(1, 100).subscribe((r) => this.customers.set(r.items));
    this.productService.getAll(1, 100).subscribe((r) => this.products.set(r.items));
    this.invoiceId = this.route.snapshot.paramMap.get('id') ?? '';
    this.isEdit = this.route.snapshot.url.some((s) => s.path === 'edit');
    if (this.isEdit && this.invoiceId) {
      this.invoiceService.getById(this.invoiceId).subscribe({
        next: (inv) => {
          this.form.patchValue({
            customerId: inv.customerId,
            invoiceDate: inv.invoiceDate,
            dueDate: inv.dueDate,
            status: inv.status,
            notes: inv.notes ?? '',
            discountAmount: inv.discountAmount,
          });
          this.items.clear();
          inv.items.forEach((item) => {
            this.items.push(this.fb.nonNullable.group({
              productId: [item.productId ?? ''],
              productName: [item.productName, Validators.required],
              description: [item.description ?? ''],
              quantity: [item.quantity, [Validators.required, Validators.min(0.01)]],
              unitPrice: [item.unitPrice, [Validators.required, Validators.min(0)]],
              discount: [item.discount],
            }));
          });
        },
        error: (err) => this.error.set(getApiErrorMessage(err)),
      });
    }
  }

  createItemGroup() {
    return this.fb.nonNullable.group({
      productId: [''],
      productName: ['', Validators.required],
      description: [''],
      quantity: [1, [Validators.required, Validators.min(0.01)]],
      unitPrice: [0, [Validators.required, Validators.min(0)]],
      discount: [0],
    });
  }

  addItem(): void { this.items.push(this.createItemGroup()); }
  removeItem(i: number): void { this.items.removeAt(i); }

  onProductSelect(index: number): void {
    const productId = this.items.at(index).get('productId')?.value;
    const product = this.products().find((p) => p.productId === productId);
    if (product) {
      this.items.at(index).patchValue({
        productName: product.productName,
        unitPrice: product.unitPrice,
        description: product.description ?? '',
      });
    }
  }

  submit(): void {
    if (this.form.invalid) return;
    this.loading.set(true);
    const raw = this.form.getRawValue();
    const payload = {
      ...raw,
      items: raw.items.map(({ productId, productName, description, quantity, unitPrice, discount }) => ({
        productId: productId || undefined,
        productName,
        description: description || undefined,
        quantity,
        unitPrice,
        discount,
      })),
    };
    const obs = this.isEdit
      ? this.invoiceService.update(this.invoiceId, payload)
      : this.invoiceService.create(payload);
    obs.subscribe({
      next: (inv) => { this.loading.set(false); this.router.navigate(['/invoices', inv.invoiceId]); },
      error: (err) => { this.loading.set(false); this.error.set(getApiErrorMessage(err, 'Save failed')); },
    });
  }
}
