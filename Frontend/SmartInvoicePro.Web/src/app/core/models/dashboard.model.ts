export interface DashboardStats {
  totalRevenue: number;
  outstandingAmount: number;
  totalInvoices: number;
  totalCustomers: number;
  overdueInvoices: number;
  paidThisMonth: number;
  totalCollected: number;
  collectionRate: number;
  averageInvoiceValue: number;
  revenueThisMonth: number;
  revenueGrowthPercent: number;
  activeProducts: number;
}

export interface MonthlyRevenue {
  month: string;
  year: number;
  revenue: number;
  collected: number;
  invoiceCount: number;
}

export interface PaymentMethodBreakdown {
  method: string;
  count: number;
  amount: number;
}

export interface AgingBucket {
  label: string;
  count: number;
  amount: number;
}

export interface TopProduct {
  productName: string;
  quantity: number;
  revenue: number;
}

export interface InvoiceStatusBreakdown {
  status: string;
  count: number;
  amount: number;
}

export interface RecentInvoice {
  invoiceId: string;
  invoiceNumber: string;
  customerName: string;
  grandTotal: number;
  status: string;
  invoiceDate: string;
}

export interface TopCustomer {
  customerId: string;
  customerName: string;
  totalRevenue: number;
  invoiceCount: number;
}

export interface Dashboard {
  stats: DashboardStats;
  monthlyRevenue: MonthlyRevenue[];
  statusBreakdown: InvoiceStatusBreakdown[];
  recentInvoices: RecentInvoice[];
  topCustomers: TopCustomer[];
  paymentMethodBreakdown: PaymentMethodBreakdown[];
  agingBuckets: AgingBucket[];
  topProducts: TopProduct[];
}
