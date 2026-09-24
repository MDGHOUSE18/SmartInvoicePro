import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CounterDirective } from '../../directives/counter.directive';

@Component({
  selector: 'app-dashboard-preview',
  standalone: true,
  imports: [CommonModule, CounterDirective],
  templateUrl: './dashboard-preview.component.html',
  styleUrl: './dashboard-preview.component.scss',
})
export class DashboardPreviewComponent {
  readonly navItems = [
    { label: 'Dashboard', icon: '◫', active: true },
    { label: 'Invoices', icon: '☰', active: false },
    { label: 'Customers', icon: '◎', active: false },
    { label: 'Payments', icon: '₹', active: false },
    { label: 'Reports', icon: '▤', active: false },
    { label: 'Settings', icon: '⚙', active: false },
  ];

  readonly metrics = [
    { label: 'Total Revenue', value: 485200, prefix: '₹', suffix: '', icon: '₹', bg: 'rgba(16,185,129,0.12)', trend: '+12.4%', up: true },
    { label: 'Invoices', value: 248, prefix: '', suffix: '', icon: '📄', bg: 'rgba(59,130,246,0.12)', trend: '+8', up: true },
    { label: 'Customers', value: 86, prefix: '', suffix: '', icon: '👥', bg: 'rgba(139,92,246,0.12)', trend: '+3', up: true },
    { label: 'Pending', value: 12, prefix: '', suffix: '', icon: '⏳', bg: 'rgba(245,158,11,0.12)', trend: '-2', up: false },
  ];

  readonly statusLegend = [
    { label: 'Paid', count: 156, color: '#10B981' },
    { label: 'Sent', count: 58, color: '#3B82F6' },
    { label: 'Overdue', count: 34, color: '#F59E0B' },
  ];

  readonly recentInvoices = [
    { id: 'INV-2026-091', customer: 'TechNova Solutions', amount: '₹42,800', status: 'Paid' },
    { id: 'INV-2026-090', customer: 'GreenLeaf Agency', amount: '₹18,500', status: 'Sent' },
    { id: 'INV-2026-089', customer: 'PixelCraft Studio', amount: '₹9,200', status: 'Paid' },
  ];
}
