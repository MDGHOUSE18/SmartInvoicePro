export interface Product {
  productId: string;
  productName: string;
  description?: string;
  unitPrice: number;
  taxPercentage: number;
  cgstPercentage: number;
  sgstPercentage: number;
  igstPercentage: number;
  status: string;
  createdDate: string;
}

export interface CreateProductRequest {
  productName: string;
  description?: string;
  unitPrice: number;
  taxPercentage: number;
  status: string;
}

export interface UpdateProductRequest extends CreateProductRequest {}
