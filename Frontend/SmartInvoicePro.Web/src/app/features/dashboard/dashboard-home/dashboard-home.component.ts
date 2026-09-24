import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { BaseChartDirective } from 'ng2-charts';
import { ChartConfiguration, Plugin, TooltipItem } from 'chart.js';
import { DashboardService } from '../../../core/services/dashboard.service';
import { AuthService } from '../../../core/services/auth.service';
import { ThemeService } from '../../../core/services/theme.service';
import { ClockService } from '../../../core/services/clock.service';
import { Dashboard } from '../../../core/models/dashboard.model';
import { getApiErrorMessage } from '../../../core/utils/api-error';
import { StatCardComponent } from '../../../shared/components/stat-card/stat-card.component';
import { StatusBadgeComponent } from '../../../shared/components/status-badge/status-badge.component';
import { formatCurrency, formatDate } from '../../../core/utils/format.utils';

interface ChartPalette {
  primary: string;
  primarySoft: string;
  accent: string;
  text: string;
  grid: string;
  surface: string;
}

const LIGHT: ChartPalette = {
  primary: '#0d9488', primarySoft: 'rgba(13,148,136,0.18)', accent: '#6366f1',
  text: '#64748b', grid: 'rgba(148,163,184,0.25)', surface: '#ffffff',
};
const DARK: ChartPalette = {
  primary: '#2dd4bf', primarySoft: 'rgba(45,212,191,0.22)', accent: '#a5b4fc',
  text: '#94a3b8', grid: 'rgba(148,163,184,0.15)', surface: '#1e293b',
};

const STATUS_COLORS: Record<string, string> = {
  Draft: '#94a3b8', Sent: '#3b82f6', Paid: '#10b981',
  PartiallyPaid: '#f59e0b', Overdue: '#ef4444', Cancelled: '#6b7280',
};
const AGING_COLORS = ['#10b981', '#84cc16', '#f59e0b', '#f97316', '#ef4444'];
const METHOD_COLORS = ['#0d9488', '#6366f1', '#f59e0b', '#ec4899', '#64748b'];
const METHOD_LABELS: Record<string, string> = {
  UPI: 'UPI', BankTransfer: 'Bank Transfer', CreditCard: 'Credit Card', Cash: 'Cash',
};

/** ₹12.3K / ₹4.5L / ₹1.2Cr for axis ticks. */
function compactInr(value: number): string {
  const abs = Math.abs(value);
  if (abs >= 1e7) return `₹${(value / 1e7).toFixed(1)}Cr`;
  if (abs >= 1e5) return `₹${(value / 1e5).toFixed(1)}L`;
  if (abs >= 1e3) return `₹${(value / 1e3).toFixed(0)}K`;
  return `₹${value}`;
}

function splitCamel(value: string): string {
  return value.replace(/([a-z])([A-Z])/g, '$1 $2');
}

@Component({
  selector: 'app-dashboard-home',
  standalone: true,
  imports: [CommonModule, RouterLink, BaseChartDirective, StatCardComponent, StatusBadgeComponent],
  templateUrl: './dashboard-home.component.html',
  styleUrl: './dashboard-home.component.scss',
})
export class DashboardHomeComponent implements OnInit {
  private readonly dashboardService = inject(DashboardService);
  private readonly theme = inject(ThemeService);
  readonly auth = inject(AuthService);
  readonly clock = inject(ClockService);

  readonly loading = signal(true);
  readonly error = signal('');
  readonly data = signal<Dashboard | null>(null);
  protected readonly formatCurrency = formatCurrency;
  protected readonly formatDate = formatDate;
  protected readonly skeletons = [1, 2, 3, 4, 5, 6];

  readonly isEmpty = computed(() => (this.data()?.stats.totalInvoices ?? 0) === 0);
  readonly growth = computed(() => this.data()?.stats.revenueGrowthPercent ?? 0);
  readonly growthText = computed(() => {
    const g = this.growth();
    if (g === 0) return 'No change vs last month';
    return `${g > 0 ? '▲' : '▼'} ${Math.abs(g)}% vs last month`;
  });

  private readonly palette = computed(() => (this.theme.isDark() ? DARK : LIGHT));

  /** Draws the invoice total in the middle of the status doughnut. */
  readonly centerTextPlugin = computed<Plugin<'doughnut'>>(() => {
    const p = this.palette();
    const total = this.data()?.stats.totalInvoices ?? 0;
    return {
      id: 'centerText',
      afterDraw: (chart) => {
        const { ctx, chartArea } = chart;
        if (!chartArea) return;
        const x = (chartArea.left + chartArea.right) / 2;
        const y = (chartArea.top + chartArea.bottom) / 2;
        ctx.save();
        ctx.textAlign = 'center';
        ctx.textBaseline = 'middle';
        ctx.fillStyle = p.text;
        ctx.font = '500 12px Inter, system-ui, sans-serif';
        ctx.fillText('Invoices', x, y + 14);
        ctx.fillStyle = p.primary;
        ctx.font = '700 24px Inter, system-ui, sans-serif';
        ctx.fillText(String(total), x, y - 6);
        ctx.restore();
      },
    };
  });

  readonly revenueChart = computed(() => {
    const d = this.data();
    const p = this.palette();
    const months = d?.monthlyRevenue ?? [];
    const data: ChartConfiguration<'bar'>['data'] = {
      labels: months.map((m) => `${m.month} ${String(m.year).slice(2)}`),
      datasets: [
        {
          label: 'Invoiced',
          data: months.map((m) => m.revenue),
          backgroundColor: p.primarySoft,
          borderColor: p.primary,
          borderWidth: 1.5,
          borderRadius: 6,
          order: 2,
        },
        {
          type: 'line',
          label: 'Collected',
          data: months.map((m) => m.collected),
          borderColor: p.accent,
          backgroundColor: p.accent,
          pointRadius: 3,
          pointHoverRadius: 5,
          tension: 0.35,
          order: 1,
        } as unknown as ChartConfiguration<'bar'>['data']['datasets'][number],
      ],
    };
    const options: ChartConfiguration<'bar'>['options'] = {
      responsive: true,
      maintainAspectRatio: false,
      interaction: { mode: 'index', intersect: false },
      plugins: {
        legend: { position: 'top', align: 'end', labels: { color: p.text, usePointStyle: true, boxWidth: 8 } },
        tooltip: { callbacks: { label: (ctx: TooltipItem<'bar'>) => ` ${ctx.dataset.label}: ${formatCurrency(ctx.parsed.y ?? 0)}` } },
      },
      scales: {
        x: { grid: { display: false }, ticks: { color: p.text } },
        y: { beginAtZero: true, grid: { color: p.grid }, border: { display: false }, ticks: { color: p.text, callback: (v) => compactInr(Number(v)) } },
      },
    };
    return { data, options };
  });

  readonly statusChart = computed(() => {
    const d = this.data();
    const p = this.palette();
    const rows = d?.statusBreakdown ?? [];
    const data: ChartConfiguration<'doughnut'>['data'] = {
      labels: rows.map((s) => splitCamel(s.status)),
      datasets: [{
        data: rows.map((s) => s.count),
        backgroundColor: rows.map((s) => STATUS_COLORS[s.status] ?? '#94a3b8'),
        borderColor: p.surface,
        borderWidth: 3,
        hoverOffset: 6,
      }],
    };
    const options: ChartConfiguration<'doughnut'>['options'] = {
      responsive: true,
      maintainAspectRatio: false,
      cutout: '68%',
      plugins: {
        legend: { position: 'bottom', labels: { color: p.text, usePointStyle: true, boxWidth: 8, padding: 14 } },
        tooltip: {
          callbacks: {
            label: (ctx: TooltipItem<'doughnut'>) => ` ${ctx.label}: ${ctx.parsed} (${formatCurrency(rows[ctx.dataIndex]?.amount ?? 0)})`,
          },
        },
      },
    };
    return { data, options };
  });

  readonly agingChart = computed(() => {
    const d = this.data();
    const p = this.palette();
    const rows = d?.agingBuckets ?? [];
    const data: ChartConfiguration<'bar'>['data'] = {
      labels: rows.map((b) => b.label),
      datasets: [{ data: rows.map((b) => b.amount), backgroundColor: AGING_COLORS, borderRadius: 6, barThickness: 22 }],
    };
    return { data, options: this.horizontalBarOptions(p, (i) => `${rows[i]?.count ?? 0} invoice(s)`) };
  });

  readonly methodChart = computed(() => {
    const d = this.data();
    const p = this.palette();
    const rows = d?.paymentMethodBreakdown ?? [];
    const data: ChartConfiguration<'pie'>['data'] = {
      labels: rows.map((m) => METHOD_LABELS[m.method] ?? splitCamel(m.method)),
      datasets: [{ data: rows.map((m) => m.amount), backgroundColor: METHOD_COLORS, borderColor: p.surface, borderWidth: 3, hoverOffset: 6 }],
    };
    const options: ChartConfiguration<'pie'>['options'] = {
      responsive: true,
      maintainAspectRatio: false,
      plugins: {
        legend: { position: 'bottom', labels: { color: p.text, usePointStyle: true, boxWidth: 8, padding: 14 } },
        tooltip: { callbacks: { label: (ctx: TooltipItem<'pie'>) => ` ${ctx.label}: ${formatCurrency(ctx.parsed)} · ${rows[ctx.dataIndex]?.count ?? 0} payment(s)` } },
      },
    };
    return { data, options };
  });

  readonly customersChart = computed(() => {
    const d = this.data();
    const p = this.palette();
    const rows = d?.topCustomers ?? [];
    const data: ChartConfiguration<'bar'>['data'] = {
      labels: rows.map((c) => c.customerName),
      datasets: [{ data: rows.map((c) => c.totalRevenue), backgroundColor: p.primary, borderRadius: 6, barThickness: 18 }],
    };
    return { data, options: this.horizontalBarOptions(p, (i) => `${rows[i]?.invoiceCount ?? 0} invoice(s)`) };
  });

  readonly productsChart = computed(() => {
    const d = this.data();
    const p = this.palette();
    const rows = d?.topProducts ?? [];
    const data: ChartConfiguration<'bar'>['data'] = {
      labels: rows.map((x) => x.productName),
      datasets: [{ data: rows.map((x) => x.revenue), backgroundColor: p.accent, borderRadius: 6, barThickness: 18 }],
    };
    return { data, options: this.horizontalBarOptions(p, (i) => `Qty ${rows[i]?.quantity ?? 0}`) };
  });

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.error.set('');
    this.dashboardService.get().subscribe({
      next: (d) => {
        this.data.set(d);
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set(getApiErrorMessage(err, 'Could not load the dashboard. Please check that the API is running.'));
        this.loading.set(false);
      },
    });
  }

  private horizontalBarOptions(p: ChartPalette, extra: (index: number) => string): ChartConfiguration<'bar'>['options'] {
    return {
      indexAxis: 'y',
      responsive: true,
      maintainAspectRatio: false,
      plugins: {
        legend: { display: false },
        tooltip: {
          callbacks: {
            label: (ctx: TooltipItem<'bar'>) => ` ${formatCurrency(ctx.parsed.x ?? 0)}`,
            afterLabel: (ctx: TooltipItem<'bar'>) => ` ${extra(ctx.dataIndex)}`,
          },
        },
      },
      scales: {
        x: { beginAtZero: true, grid: { color: p.grid }, border: { display: false }, ticks: { color: p.text, callback: (v) => compactInr(Number(v)) } },
        y: { grid: { display: false }, ticks: { color: p.text } },
      },
    };
  }
}
