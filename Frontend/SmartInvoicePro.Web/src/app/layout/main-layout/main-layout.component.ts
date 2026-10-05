import { Component, DestroyRef, HostListener, inject, signal } from '@angular/core';
import { NavigationEnd, Router, RouterOutlet } from '@angular/router';
import { CommonModule } from '@angular/common';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { filter } from 'rxjs';
import { SidebarComponent } from '../sidebar/sidebar.component';
import { HeaderComponent } from '../header/header.component';
import { FooterComponent } from '../footer/footer.component';
import { WELCOME_FLAG_KEY, WelcomeDialogComponent } from '../../shared/components/welcome-dialog/welcome-dialog.component';

// Keep in sync with the `max-width: 768px` media queries in the layout styles.
const MOBILE_QUERY = '(max-width: 768px)';
const isMobileViewport = () => window.matchMedia(MOBILE_QUERY).matches;

@Component({
  selector: 'app-main-layout',
  standalone: true,
  imports: [CommonModule, RouterOutlet, SidebarComponent, HeaderComponent, FooterComponent, WelcomeDialogComponent],
  templateUrl: './main-layout.component.html',
  styleUrl: './main-layout.component.scss',
})
export class MainLayoutComponent {
  readonly isMobile = signal(isMobileViewport());
  readonly sidebarCollapsed = signal(this.isMobile());
  readonly mobileOpen = signal(false);
  readonly showWelcome = signal(this.consumeWelcomeFlag());

  constructor() {
    inject(Router)
      .events.pipe(
        filter((e) => e instanceof NavigationEnd),
        takeUntilDestroyed(inject(DestroyRef)),
      )
      .subscribe(() => {
        if (this.isMobile()) this.closeMobile();
      });
  }

  @HostListener('window:resize')
  onResize(): void {
    const mobile = isMobileViewport();
    if (mobile === this.isMobile()) return;
    this.isMobile.set(mobile);
    this.mobileOpen.set(false);
    this.sidebarCollapsed.set(mobile);
  }

  private consumeWelcomeFlag(): boolean {
    const show = sessionStorage.getItem(WELCOME_FLAG_KEY) === '1';
    sessionStorage.removeItem(WELCOME_FLAG_KEY);
    return show;
  }

  toggleSidebar(): void {
    if (this.isMobile()) {
      this.mobileOpen.update((v) => !v);
      this.sidebarCollapsed.set(!this.mobileOpen());
    } else {
      this.sidebarCollapsed.update((v) => !v);
    }
  }

  closeMobile(): void {
    this.mobileOpen.set(false);
    this.sidebarCollapsed.set(true);
  }
}
