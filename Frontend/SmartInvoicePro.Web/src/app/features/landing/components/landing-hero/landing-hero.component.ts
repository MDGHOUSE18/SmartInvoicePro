import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AnimateOnScrollDirective } from '../../directives/animate-on-scroll.directive';
import { RippleDirective } from '../../directives/ripple.directive';
import { DashboardPreviewComponent } from '../dashboard-preview/dashboard-preview.component';

@Component({
  selector: 'app-landing-hero',
  standalone: true,
  imports: [RouterLink, AnimateOnScrollDirective, RippleDirective, DashboardPreviewComponent],
  templateUrl: './landing-hero.component.html',
  styleUrl: './landing-hero.component.scss',
})
export class LandingHeroComponent {
  readonly trustItems = ['No Credit Card', 'Free Demo', 'GST Ready', 'Secure Cloud'];
}
