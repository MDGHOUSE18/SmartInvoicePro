import { Injectable, inject } from '@angular/core';
import { ApiService } from './api.service';
import { Product, CreateProductRequest, UpdateProductRequest } from '../models/product.model';
import { PagedResult } from '../models/api-response.model';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class ProductService {
  private readonly api = inject(ApiService);

  getAll(page = 1, pageSize = 20, search?: string): Observable<PagedResult<Product>> {
    return this.api.get<PagedResult<Product>>('products', { page, pageSize, search });
  }

  getById(id: string): Observable<Product> {
    return this.api.get<Product>(`products/${id}`);
  }

  create(dto: CreateProductRequest): Observable<Product> {
    return this.api.post<Product>('products', dto);
  }

  update(id: string, dto: UpdateProductRequest): Observable<Product> {
    return this.api.put<Product>(`products/${id}`, dto);
  }

  delete(id: string): Observable<void> {
    return this.api.delete(`products/${id}`);
  }
}
