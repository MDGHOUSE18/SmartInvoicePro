import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-landing-footer',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './landing-footer.component.html',
  styleUrl: './landing-footer.component.scss',
})
export class LandingFooterComponent {
  readonly year = new Date().getFullYear();

  readonly socials = [
    { label: 'GitHub', href: 'https://github.com', icon: '⌘' },
    { label: 'LinkedIn', href: 'https://linkedin.com', icon: 'in' },
    { label: 'Twitter', href: 'https://twitter.com', icon: '𝕏' },
  ];

  readonly linkGroups = [
    {
      title: 'Company',
      links: [
        { label: 'About', href: '#home' },
        { label: 'Features', href: '#features' },
        { label: 'Pricing', href: '#pricing' },
      ],
    },
    {
      title: 'Support',
      links: [
        { label: 'Documentation', href: '#', external: false },
        { label: 'Contact', href: '#contact' },
        { label: 'FAQ', href: '#faq' },
      ],
    },
    {
      title: 'Resources',
      links: [
        { label: 'GitHub', href: 'https://github.com', external: true },
        { label: 'LinkedIn', href: 'https://linkedin.com', external: true },
        { label: 'Login', href: '/auth/login' },
      ],
    },
  ];
}
