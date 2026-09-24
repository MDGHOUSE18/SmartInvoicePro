import { Component, signal } from '@angular/core';
import { AnimateOnScrollDirective } from '../../directives/animate-on-scroll.directive';

interface FaqItem {
  question: string;
  answer: string;
}

@Component({
  selector: 'app-landing-faq',
  standalone: true,
  imports: [AnimateOnScrollDirective],
  templateUrl: './landing-faq.component.html',
  styleUrl: './landing-faq.component.scss',
})
export class LandingFaqComponent {
  readonly openIndex = signal<number | null>(0);

  readonly faqs: FaqItem[] = [
    {
      question: 'Is SmartInvoice Pro free?',
      answer: 'Yes! Our Starter plan is completely free with unlimited invoices and customers. You can upgrade to Professional or Enterprise plans anytime for advanced features and priority support.',
    },
    {
      question: 'Can I export PDF?',
      answer: 'Absolutely. Every invoice and report can be exported as a polished, print-ready PDF with your branding, logo, and custom formatting in just one click.',
    },
    {
      question: 'Does it support GST?',
      answer: 'Yes, SmartInvoice Pro includes built-in GST calculation and tax-ready invoice formats, making compliance straightforward for businesses operating in GST-applicable regions.',
    },
    {
      question: 'Can I manage customers?',
      answer: 'Our customer management module lets you store contact details, billing history, payment preferences, and notes—all organized in searchable profiles.',
    },
    {
      question: 'Is data secure?',
      answer: 'Your data is protected with bank-level encryption, secure cloud infrastructure, regular backups, and role-based access controls. We take security seriously.',
    },
  ];

  toggle(index: number): void {
    this.openIndex.update((current) => (current === index ? null : index));
  }
}
