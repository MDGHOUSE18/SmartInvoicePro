import { Component, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { ProductService } from '../../../core/services/product.service';
import { getApiErrorMessage } from '../../../core/utils/api-error';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';

@Component({
  selector: 'app-product-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink, PageHeaderComponent],
  templateUrl: './product-form.component.html',
  styleUrl: './product-form.component.scss',
})
export class ProductFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly productService = inject(ProductService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  readonly loading = signal(false);
  readonly error = signal('');
  isEdit = false;
  productId = '';

  readonly form = this.fb.nonNullable.group({
    productName: ['', Validators.required],
    description: [''],
    unitPrice: [0, [Validators.required, Validators.min(0)]],
    taxPercentage: [18, [Validators.required, Validators.min(0)]],
    status: ['Active'],
  });

  ngOnInit(): void {
    this.productId = this.route.snapshot.paramMap.get('id') ?? '';
    this.isEdit = this.route.snapshot.url.some((s) => s.path === 'edit');
    if (this.isEdit && this.productId) {
      this.productService.getById(this.productId).subscribe({
        next: (p) => this.form.patchValue(p),
        error: (err) => this.error.set(getApiErrorMessage(err)),
      });
    }
  }

  submit(): void {
    if (this.form.invalid) return;
    this.loading.set(true);
    const raw = this.form.getRawValue();
    const obs = this.isEdit ? this.productService.update(this.productId, raw) : this.productService.create(raw);
    obs.subscribe({
      next: () => { this.loading.set(false); this.router.navigate(['/products']); },
      error: (err) => { this.loading.set(false); this.error.set(getApiErrorMessage(err, 'Save failed')); },
    });
  }
}
