export interface InvoiceItem {
  invoiceItemId?: string;
  productId?: string;
  productName: string;
  description?: string;
  quantity: number;
  unitPrice: number;
  cgstPercentage: number;
  sgstPercentage: number;
  igstPercentage: number;
  discount: number;
  lineTotal: number;
}

export interface Invoice {
  invoiceId: string;
  invoiceNumber: string;
  invoiceDate: string;
  dueDate: string;
  customerId: string;
  customerName: string;
  status: string;
  taxType: string;
  currencyCode: string;
  notes?: string;
  subtotal: number;
  cgstAmount: number;
  sgstAmount: number;
  igstAmount: number;
  taxAmount: number;
  discountAmount: number;
  grandTotal: number;
  amountPaid: number;
  balanceAmount: number;
  items: InvoiceItem[];
  createdDate: string;
}

export interface InvoiceListItem {
  invoiceId: string;
  invoiceNumber: string;
  invoiceDate: string;
  dueDate: string;
  customerName: string;
  status: string;
  grandTotal: number;
  balanceAmount: number;
}

export interface CreateInvoiceItemRequest {
  productId?: string;
  productName: string;
  description?: string;
  quantity: number;
  unitPrice: number;
  discount: number;
}

export interface CreateInvoiceRequest {
  invoiceDate: string;
  dueDate: string;
  customerId: string;
  notes?: string;
  discountAmount: number;
  items: CreateInvoiceItemRequest[];
}

export interface UpdateInvoiceRequest extends CreateInvoiceRequest {
  status: string;
}

export interface InvoiceSummary {
  totalInvoices: number;
  totalRevenue: number;
  totalOutstanding: number;
  draftCount: number;
  sentCount: number;
  paidCount: number;
  overdueCount: number;
}

export const INVOICE_STATUSES = ['Draft', 'Sent', 'Paid', 'Overdue', 'Cancelled'] as const;
export type InvoiceStatus = (typeof INVOICE_STATUSES)[number];
