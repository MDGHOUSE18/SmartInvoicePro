import { Directive, ElementRef, inject, input, OnDestroy, OnInit, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';

@Directive({
  selector: '[appCounter]',
  standalone: true,
})
export class CounterDirective implements OnInit, OnDestroy {
  private readonly el = inject(ElementRef<HTMLElement>);
  private readonly platformId = inject(PLATFORM_ID);

  readonly appCounter = input(0);
  readonly counterDuration = input(2000);
  readonly counterPrefix = input('');
  readonly counterSuffix = input('');

  private observer?: IntersectionObserver;
  private frameId?: number;

  ngOnInit(): void {
    if (!isPlatformBrowser(this.platformId)) {
      this.el.nativeElement.textContent = this.formatValue(this.appCounter());
      return;
    }

    const element = this.el.nativeElement;
    element.textContent = this.formatValue(0);

    this.observer = new IntersectionObserver(
      (entries) => {
        if (entries.some((e) => e.isIntersecting)) {
          this.animate();
          this.observer?.disconnect();
        }
      },
      { threshold: 0.5 }
    );

    this.observer.observe(element);
  }

  private animate(): void {
    const target = this.appCounter();
    const duration = this.counterDuration();
    const start = performance.now();

    const step = (now: number) => {
      const progress = Math.min((now - start) / duration, 1);
      const eased = 1 - Math.pow(1 - progress, 3);
      const value = Math.round(target * eased);
      this.el.nativeElement.textContent = this.formatValue(value);

      if (progress < 1) {
        this.frameId = requestAnimationFrame(step);
      }
    };

    this.frameId = requestAnimationFrame(step);
  }

  private formatValue(value: number): string {
    return `${this.counterPrefix()}${value.toLocaleString()}${this.counterSuffix()}`;
  }

  ngOnDestroy(): void {
    this.observer?.disconnect();
    if (this.frameId) cancelAnimationFrame(this.frameId);
  }
}
