import { Component, inject, OnInit } from '@angular/core';
import { Meta, Title } from '@angular/platform-browser';
import { LandingNavbarComponent } from './components/landing-navbar/landing-navbar.component';
import { LandingHeroComponent } from './components/landing-hero/landing-hero.component';
import { LandingTrustedComponent } from './components/landing-trusted/landing-trusted.component';
import { LandingFeaturesComponent } from './components/landing-features/landing-features.component';
import { LandingShowcaseComponent } from './components/landing-showcase/landing-showcase.component';
import { LandingWhyChooseComponent } from './components/landing-why-choose/landing-why-choose.component';
import { LandingWorkflowComponent } from './components/landing-workflow/landing-workflow.component';
import { LandingPricingComponent } from './components/landing-pricing/landing-pricing.component';
import { LandingTestimonialsComponent } from './components/landing-testimonials/landing-testimonials.component';
import { LandingFaqComponent } from './components/landing-faq/landing-faq.component';
import { LandingCtaComponent } from './components/landing-cta/landing-cta.component';
import { LandingFooterComponent } from './components/landing-footer/landing-footer.component';

@Component({
  selector: 'app-landing-page',
  standalone: true,
  imports: [
    LandingNavbarComponent,
    LandingHeroComponent,
    LandingTrustedComponent,
    LandingFeaturesComponent,
    LandingShowcaseComponent,
    LandingWhyChooseComponent,
    LandingWorkflowComponent,
    LandingPricingComponent,
    LandingTestimonialsComponent,
    LandingFaqComponent,
    LandingCtaComponent,
    LandingFooterComponent,
  ],
  templateUrl: './landing-page.component.html',
  styleUrl: './landing-page.component.scss',
})
export class LandingPageComponent implements OnInit {
  private readonly title = inject(Title);
  private readonly meta = inject(Meta);

  ngOnInit(): void {
    this.title.setTitle('SmartInvoice Pro — Professional Invoice & Business Management');
    this.meta.updateTag({
      name: 'description',
      content:
        'SmartInvoice Pro helps freelancers and businesses create invoices, manage customers, track payments, and generate reports—all in one secure platform.',
    });
    this.meta.updateTag({ property: 'og:title', content: 'SmartInvoice Pro' });
    this.meta.updateTag({
      property: 'og:description',
      content: 'Professional Invoice & Business Management System for Modern Businesses',
    });
  }
}
