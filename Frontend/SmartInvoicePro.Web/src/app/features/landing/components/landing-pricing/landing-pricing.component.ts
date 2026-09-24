import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AnimateOnScrollDirective } from '../../directives/animate-on-scroll.directive';
import { RippleDirective } from '../../directives/ripple.directive';

interface PricingPlan {
  name: string;
  price: string;
  period: string;
  description: string;
  features: string[];
  highlighted: boolean;
}

@Component({
  selector: 'app-landing-pricing',
  standalone: true,
  imports: [RouterLink, AnimateOnScrollDirective, RippleDirective],
  templateUrl: './landing-pricing.component.html',
  styleUrl: './landing-pricing.component.scss',
})
export class LandingPricingComponent {
  readonly plans: PricingPlan[] = [
    {
      name: 'Starter',
      price: '₹0',
      period: '/month',
      description: 'Perfect for freelancers just getting started.',
      features: ['Unlimited Invoices', 'Unlimited Customers', 'Basic Reports', 'PDF Export', 'Email Support'],
      highlighted: false,
    },
    {
      name: 'Professional',
      price: '₹999',
      period: '/month',
      description: 'For growing businesses that need more power.',
      features: ['Unlimited Invoices', 'Unlimited Customers', 'Advanced Reports', 'PDF Export', 'Priority Support'],
      highlighted: true,
    },
    {
      name: 'Enterprise',
      price: '₹2,499',
      period: '/month',
      description: 'For teams and agencies with advanced needs.',
      features: ['Unlimited Invoices', 'Unlimited Customers', 'Custom Reports', 'PDF Export', 'Priority Support'],
      highlighted: false,
    },
  ];
}
