import { Directive, ElementRef, inject, input, OnDestroy, OnInit, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';

export type AnimateType = 'fade-up' | 'fade-left' | 'fade-right' | 'scale';

@Directive({
  selector: '[appAnimateOnScroll]',
  standalone: true,
})
export class AnimateOnScrollDirective implements OnInit, OnDestroy {
  private readonly el = inject(ElementRef<HTMLElement>);
  private readonly platformId = inject(PLATFORM_ID);

  readonly appAnimateOnScroll = input<AnimateType>('fade-up');
  readonly animateDelay = input(0);

  private observer?: IntersectionObserver;

  ngOnInit(): void {
    if (!isPlatformBrowser(this.platformId)) return;

    const element = this.el.nativeElement;
    const type = this.appAnimateOnScroll();
    const delay = this.animateDelay();

    element.classList.add('landing-animate', `landing-animate--${type}`);
    if (delay > 0) {
      element.style.transitionDelay = `${delay}ms`;
    }

    this.observer = new IntersectionObserver(
      (entries) => {
        entries.forEach((entry) => {
          if (entry.isIntersecting) {
            entry.target.classList.add('landing-animate--visible');
            this.observer?.unobserve(entry.target);
          }
        });
      },
      { threshold: 0.12, rootMargin: '0px 0px -40px 0px' }
    );

    this.observer.observe(element);
  }

  ngOnDestroy(): void {
    this.observer?.disconnect();
  }
}
