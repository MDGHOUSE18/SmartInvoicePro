import { Invoice, InvoiceItem } from '../models/invoice.model';

type TaxField = 'cgstPercentage' | 'sgstPercentage' | 'igstPercentage';

function uniqueTaxRates(items: InvoiceItem[], field: TaxField): number[] {
  return [...new Set(items.map((item) => item[field]).filter((rate) => rate > 0))].sort((a, b) => a - b);
}

function derivedRate(amount: number, subtotal: number): number | null {
  if (amount <= 0 || subtotal <= 0) return null;
  return Math.round((amount / subtotal) * 10000) / 100;
}

function summaryTaxRates(type: 'CGST' | 'SGST' | 'IGST', invoice: Invoice): number[] {
  const field: TaxField =
    type === 'CGST' ? 'cgstPercentage' : type === 'SGST' ? 'sgstPercentage' : 'igstPercentage';
  const fromItems = uniqueTaxRates(invoice.items, field);
  if (fromItems.length) return fromItems;

  const amount =
    type === 'CGST' ? invoice.cgstAmount : type === 'SGST' ? invoice.sgstAmount : invoice.igstAmount;
  const derived = derivedRate(amount, invoice.subtotal);
  return derived !== null ? [derived] : [];
}

/** Summary label e.g. "CGST (9%)" */
export function invoiceTaxLabel(type: 'CGST' | 'SGST' | 'IGST', invoice: Invoice): string {
  const rates = summaryTaxRates(type, invoice);
  if (!rates.length) return type;
  if (rates.length === 1) return `${type} (${rates[0]}%)`;
  return `${type} (${rates.map((r) => `${r}%`).join(', ')})`;
}

/** Per line-item tax rate e.g. "CGST 9% + SGST 9%" */
export function formatItemTaxRate(item: InvoiceItem, taxType: string): string {
  const usesIgst =
    taxType.toUpperCase().includes('IGST') &&
    !taxType.toUpperCase().includes('CGST') &&
    !taxType.toUpperCase().includes('SGST');

  if (usesIgst || (item.igstPercentage > 0 && item.cgstPercentage === 0 && item.sgstPercentage === 0)) {
    return item.igstPercentage > 0 ? `IGST ${item.igstPercentage}%` : '—';
  }

  const parts: string[] = [];
  if (item.cgstPercentage > 0) parts.push(`CGST ${item.cgstPercentage}%`);
  if (item.sgstPercentage > 0) parts.push(`SGST ${item.sgstPercentage}%`);
  if (!parts.length && item.igstPercentage > 0) parts.push(`IGST ${item.igstPercentage}%`);
  return parts.length ? parts.join(' + ') : '—';
}
