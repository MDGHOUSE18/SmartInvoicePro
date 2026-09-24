export function formatCurrency(amount: number): string {
  return new Intl.NumberFormat('en-IN', {
    style: 'currency',
    currency: 'INR',
    minimumFractionDigits: 2,
  }).format(amount);
}

export function formatDate(date: string | Date): string {
  const d = typeof date === 'string' ? new Date(date) : date;
  return d.toLocaleDateString('en-IN', { day: '2-digit', month: 'short', year: 'numeric' });
}

export function toDateInputValue(date: string | Date): string {
  const d = typeof date === 'string' ? new Date(date) : date;
  return d.toISOString().split('T')[0];
}

export function todayIso(): string {
  return new Date().toISOString().split('T')[0];
}

export function downloadBlob(blob: Blob, filename: string): void {
  const url = URL.createObjectURL(blob);
  const a = document.createElement('a');
  a.href = url;
  a.download = filename;
  a.click();
  URL.revokeObjectURL(url);
}

export function getStatusClass(status: string): string {
  const map: Record<string, string> = {
    Draft: 'status-draft',
    Sent: 'status-sent',
    Paid: 'status-paid',
    Overdue: 'status-overdue',
    Cancelled: 'status-cancelled',
    Active: 'status-paid',
    Inactive: 'status-cancelled',
  };
  return map[status] ?? 'status-draft';
}
