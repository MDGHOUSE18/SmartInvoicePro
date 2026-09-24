import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ProductService } from '../../../core/services/product.service';
import { Product } from '../../../core/models/product.model';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { StatusBadgeComponent } from '../../../shared/components/status-badge/status-badge.component';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog/confirm-dialog.component';
import { formatCurrency } from '../../../core/utils/format.utils';

@Component({
  selector: 'app-product-list',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule, PageHeaderComponent, StatusBadgeComponent, ConfirmDialogComponent],
  templateUrl: './product-list.component.html',
  styleUrl: './product-list.component.scss',
})
export class ProductListComponent implements OnInit {
  private readonly productService = inject(ProductService);
  readonly products = signal<Product[]>([]);
  readonly loading = signal(false);
  readonly deleteOpen = signal(false);
  private deleteTarget: Product | null = null;
  search = '';
  protected readonly formatCurrency = formatCurrency;

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading.set(true);
    this.productService.getAll(1, 50, this.search || undefined).subscribe({
      next: (r) => { this.products.set(r.items); this.loading.set(false); },
      error: () => this.loading.set(false),
    });
  }

  onSearch(): void { this.load(); }
  confirmDelete(p: Product): void { this.deleteTarget = p; this.deleteOpen.set(true); }
  doDelete(): void {
    if (!this.deleteTarget) return;
    this.productService.delete(this.deleteTarget.productId).subscribe({
      next: () => { this.deleteOpen.set(false); this.load(); },
    });
  }
}
