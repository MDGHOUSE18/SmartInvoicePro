export interface Payment {
  paymentId: string;
  invoiceId: string;
  invoiceNumber: string;
  paymentDate: string;
  amountPaid: number;
  paymentMethod: string;
  balanceAmount: number;
  referenceNumber?: string;
  notes?: string;
  createdDate: string;
}

export interface CreatePaymentRequest {
  invoiceId: string;
  paymentDate: string;
  amountPaid: number;
  paymentMethod: string;
  referenceNumber?: string;
  notes?: string;
}

export const PAYMENT_METHODS = ['Cash', 'Bank Transfer', 'UPI', 'Cheque', 'Credit Card', 'Other'] as const;
