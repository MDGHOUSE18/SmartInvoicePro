export interface ReportFilter {
  fromDate?: string;
  toDate?: string;
  status?: string;
  customerId?: string;
}

export interface SalesReportLine {
  invoiceDate: string;
  invoiceNumber: string;
  customerName: string;
  status: string;
  subtotal: number;
  taxAmount: number;
  grandTotal: number;
  amountPaid: number;
}

export interface SalesReport {
  fromDate: string;
  toDate: string;
  totalInvoices: number;
  totalSales: number;
  totalTax: number;
  totalDiscount: number;
  totalCollected: number;
  totalOutstanding: number;
  lines: SalesReportLine[];
}

export interface TaxReportLine {
  invoiceNumber: string;
  invoiceDate: string;
  customerName: string;
  taxType: string;
  cgstAmount: number;
  sgstAmount: number;
  igstAmount: number;
  taxAmount: number;
}

export interface TaxReport {
  fromDate: string;
  toDate: string;
  totalCgst: number;
  totalSgst: number;
  totalIgst: number;
  totalTax: number;
  lines: TaxReportLine[];
}

export interface CustomerReport {
  customerId: string;
  customerName: string;
  invoiceCount: number;
  totalBilled: number;
  totalPaid: number;
  outstanding: number;
}

export interface PaymentReportLine {
  paymentDate: string;
  invoiceNumber: string;
  customerName: string;
  paymentMethod: string;
  amountPaid: number;
  referenceNumber?: string;
}

export interface PaymentReport {
  fromDate: string;
  toDate: string;
  totalCollected: number;
  lines: PaymentReportLine[];
}
