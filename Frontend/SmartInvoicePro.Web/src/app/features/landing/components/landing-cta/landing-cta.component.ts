import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AnimateOnScrollDirective } from '../../directives/animate-on-scroll.directive';
import { RippleDirective } from '../../directives/ripple.directive';

@Component({
  selector: 'app-landing-cta',
  standalone: true,
  imports: [RouterLink, AnimateOnScrollDirective, RippleDirective],
  templateUrl: './landing-cta.component.html',
  styleUrl: './landing-cta.component.scss',
})
export class LandingCtaComponent {}
