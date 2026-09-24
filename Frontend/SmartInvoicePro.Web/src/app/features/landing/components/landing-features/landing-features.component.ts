import { Component } from '@angular/core';
import { AnimateOnScrollDirective } from '../../directives/animate-on-scroll.directive';

interface Feature {
  title: string;
  description: string;
  icon: string;
  color: string;
}

@Component({
  selector: 'app-landing-features',
  standalone: true,
  imports: [AnimateOnScrollDirective],
  templateUrl: './landing-features.component.html',
  styleUrl: './landing-features.component.scss',
})
export class LandingFeaturesComponent {
  readonly features: Feature[] = [
    {
      title: 'Professional Invoices',
      description: 'Create branded invoices with custom templates, line items, and automatic numbering in seconds.',
      icon: '📋',
      color: '#10B981',
    },
    {
      title: 'Customer Management',
      description: 'Organize contacts, track billing history, and maintain detailed customer profiles effortlessly.',
      icon: '👥',
      color: '#3B82F6',
    },
    {
      title: 'Expense Tracking',
      description: 'Log business expenses, categorize spending, and keep your finances organized in one place.',
      icon: '💳',
      color: '#8B5CF6',
    },
    {
      title: 'Payment Tracking',
      description: 'Monitor incoming payments, send reminders, and reconcile accounts with real-time updates.',
      icon: '💰',
      color: '#F59E0B',
    },
    {
      title: 'GST Calculation',
      description: 'Automatic tax calculations with GST-ready formats for compliant invoicing across regions.',
      icon: '🧾',
      color: '#EF4444',
    },
    {
      title: 'Reports & Analytics',
      description: 'Visual dashboards and detailed reports to understand revenue trends and business performance.',
      icon: '📊',
      color: '#06B6D4',
    },
    {
      title: 'PDF Export',
      description: 'Export polished, print-ready PDF invoices and reports with a single click.',
      icon: '📄',
      color: '#EC4899',
    },
    {
      title: 'Email Invoices',
      description: 'Send invoices directly to clients with delivery tracking and automated follow-up reminders.',
      icon: '✉️',
      color: '#6366F1',
    },
  ];
}
