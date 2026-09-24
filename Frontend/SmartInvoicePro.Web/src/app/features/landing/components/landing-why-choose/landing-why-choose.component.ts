import { Component } from '@angular/core';
import { AnimateOnScrollDirective } from '../../directives/animate-on-scroll.directive';

@Component({
  selector: 'app-landing-why-choose',
  standalone: true,
  imports: [AnimateOnScrollDirective],
  templateUrl: './landing-why-choose.component.html',
  styleUrl: './landing-why-choose.component.scss',
})
export class LandingWhyChooseComponent {
  readonly items = [
    {
      title: 'Save Time',
      description: 'Automate repetitive billing tasks and cut invoice creation time by up to 80% with smart templates.',
      emoji: '⚡',
      gradient: 'linear-gradient(135deg, rgba(16,185,129,0.15), rgba(6,182,212,0.1))',
    },
    {
      title: 'Secure Data',
      description: 'Bank-level encryption, secure cloud storage, and role-based access keep your business data protected.',
      emoji: '🔒',
      gradient: 'linear-gradient(135deg, rgba(59,130,246,0.15), rgba(99,102,241,0.1))',
    },
    {
      title: 'Easy Billing',
      description: 'Intuitive interface designed for non-accountants. Get started in minutes, not days.',
      emoji: '✨',
      gradient: 'linear-gradient(135deg, rgba(245,158,11,0.15), rgba(251,191,36,0.1))',
    },
    {
      title: 'Business Insights',
      description: 'Actionable analytics and reports help you make informed decisions and spot growth opportunities.',
      emoji: '📈',
      gradient: 'linear-gradient(135deg, rgba(139,92,246,0.15), rgba(236,72,153,0.1))',
    },
  ];
}
