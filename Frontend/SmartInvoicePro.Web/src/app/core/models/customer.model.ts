export interface Customer {
  customerId: string;
  customerName: string;
  companyName?: string;
  email: string;
  phone?: string;
  address?: string;
  city?: string;
  state?: string;
  country: string;
  taxNumber?: string;
  isActive: boolean;
  createdDate: string;
}

export interface CustomerListItem {
  customerId: string;
  customerName: string;
  companyName?: string;
  email: string;
  phone?: string;
  city?: string;
  state?: string;
  isActive: boolean;
  invoiceCount: number;
}

export interface CreateCustomerRequest {
  customerName: string;
  companyName?: string;
  email: string;
  phone?: string;
  address?: string;
  city?: string;
  state?: string;
  country: string;
  taxNumber?: string;
}

export interface UpdateCustomerRequest extends CreateCustomerRequest {
  isActive: boolean;
}
