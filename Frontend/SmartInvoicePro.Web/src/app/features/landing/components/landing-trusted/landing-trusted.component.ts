import { Component } from '@angular/core';
import { AnimateOnScrollDirective } from '../../directives/animate-on-scroll.directive';

@Component({
  selector: 'app-landing-trusted',
  standalone: true,
  imports: [AnimateOnScrollDirective],
  templateUrl: './landing-trusted.component.html',
  styleUrl: './landing-trusted.component.scss',
})
export class LandingTrustedComponent {
  readonly companies = [
    { name: 'Microsoft' },
    { name: 'Google' },
    { name: 'Amazon' },
    { name: 'Adobe' },
    { name: 'Spotify' },
    { name: 'Slack' },
    { name: 'Stripe' },
  ];
}
