import { Component } from '@angular/core';
import { AnimateOnScrollDirective } from '../../directives/animate-on-scroll.directive';

interface Testimonial {
  name: string;
  company: string;
  role: string;
  review: string;
  rating: number;
  initials: string;
  color: string;
}

@Component({
  selector: 'app-landing-testimonials',
  standalone: true,
  imports: [AnimateOnScrollDirective],
  templateUrl: './landing-testimonials.component.html',
  styleUrl: './landing-testimonials.component.scss',
})
export class LandingTestimonialsComponent {
  readonly testimonials: Testimonial[] = [
    {
      name: 'Priya Sharma',
      company: 'DesignCraft Studio',
      role: 'Founder',
      review: 'SmartInvoice Pro cut my invoicing time in half. The dashboard gives me a clear picture of my cash flow every morning.',
      rating: 5,
      initials: 'PS',
      color: 'linear-gradient(135deg, #10b981, #059669)',
    },
    {
      name: 'James Mitchell',
      company: 'Mitchell Consulting',
      role: 'Managing Partner',
      review: 'We switched from spreadsheets and never looked back. GST calculations are spot-on and our clients love the professional PDFs.',
      rating: 5,
      initials: 'JM',
      color: 'linear-gradient(135deg, #3b82f6, #6366f1)',
    },
    {
      name: 'Sarah Chen',
      company: 'PixelWave Agency',
      role: 'Operations Lead',
      review: 'The customer management and payment tracking features are exactly what our agency needed. Setup took less than 10 minutes.',
      rating: 5,
      initials: 'SC',
      color: 'linear-gradient(135deg, #8b5cf6, #ec4899)',
    },
  ];
}
