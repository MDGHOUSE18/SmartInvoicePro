import { Component } from '@angular/core';
import { AnimateOnScrollDirective } from '../../directives/animate-on-scroll.directive';

@Component({
  selector: 'app-landing-showcase',
  standalone: true,
  imports: [AnimateOnScrollDirective],
  templateUrl: './landing-showcase.component.html',
  styleUrl: './landing-showcase.component.scss',
})
export class LandingShowcaseComponent {
  readonly stats = [
    { label: 'Total Revenue', value: '₹4,85,200', bar: 85 },
    { label: 'Outstanding', value: '₹42,800', bar: 35 },
    { label: 'Collected', value: '₹4,42,400', bar: 78 },
  ];

  readonly barData = [
    { month: 'Jan', height: 45 },
    { month: 'Feb', height: 62 },
    { month: 'Mar', height: 55 },
    { month: 'Apr', height: 78 },
    { month: 'May', height: 70 },
    { month: 'Jun', height: 92 },
  ];
}
