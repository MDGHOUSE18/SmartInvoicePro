import { Component, HostListener, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { ThemeService } from '../../../../core/services/theme.service';
import { RippleDirective } from '../../directives/ripple.directive';

@Component({
  selector: 'app-landing-navbar',
  standalone: true,
  imports: [CommonModule, RouterLink, RippleDirective],
  templateUrl: './landing-navbar.component.html',
  styleUrl: './landing-navbar.component.scss',
})
export class LandingNavbarComponent {
  protected readonly theme = inject(ThemeService);
  readonly scrolled = signal(false);
  readonly mobileOpen = signal(false);

  readonly navLinks = [
    { label: 'Home', href: '#home' },
    { label: 'Features', href: '#features' },
    { label: 'Solutions', href: '#solutions' },
    { label: 'Pricing', href: '#pricing' },
    { label: 'FAQ', href: '#faq' },
    { label: 'Contact', href: '#contact' },
  ];

  @HostListener('window:scroll')
  onScroll(): void {
    this.scrolled.set(window.scrollY > 20);
  }

  closeMobile(): void {
    this.mobileOpen.set(false);
  }
}
