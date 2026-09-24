import { Directive, ElementRef, HostListener, inject, Renderer2 } from '@angular/core';

@Directive({
  selector: '[appRipple]',
  standalone: true,
})
export class RippleDirective {
  private readonly el = inject(ElementRef<HTMLElement>);
  private readonly renderer = inject(Renderer2);

  @HostListener('click', ['$event'])
  onClick(event: MouseEvent): void {
    const button = this.el.nativeElement;
    const rect = button.getBoundingClientRect();
    const size = Math.max(rect.width, rect.height);
    const x = event.clientX - rect.left - size / 2;
    const y = event.clientY - rect.top - size / 2;

    const ripple = this.renderer.createElement('span');
    this.renderer.addClass(ripple, 'landing-ripple');
    this.renderer.setStyle(ripple, 'width', `${size}px`);
    this.renderer.setStyle(ripple, 'height', `${size}px`);
    this.renderer.setStyle(ripple, 'left', `${x}px`);
    this.renderer.setStyle(ripple, 'top', `${y}px`);

    this.renderer.appendChild(button, ripple);
    ripple.addEventListener('animationend', () => ripple.remove());
  }
}
