import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { CommonModule } from '@angular/common';
import { SidebarComponent } from '../sidebar/sidebar.component';
import { HeaderComponent } from '../header/header.component';
import { FooterComponent } from '../footer/footer.component';
import { WELCOME_FLAG_KEY, WelcomeDialogComponent } from '../../shared/components/welcome-dialog/welcome-dialog.component';

@Component({
  selector: 'app-main-layout',
  standalone: true,
  imports: [CommonModule, RouterOutlet, SidebarComponent, HeaderComponent, FooterComponent, WelcomeDialogComponent],
  templateUrl: './main-layout.component.html',
  styleUrl: './main-layout.component.scss',
})
export class MainLayoutComponent {
  readonly sidebarCollapsed = signal(window.innerWidth < 768);
  readonly mobileOpen = signal(false);
  readonly showWelcome = signal(this.consumeWelcomeFlag());

  private consumeWelcomeFlag(): boolean {
    const show = sessionStorage.getItem(WELCOME_FLAG_KEY) === '1';
    sessionStorage.removeItem(WELCOME_FLAG_KEY);
    return show;
  }

  toggleSidebar(): void {
    if (window.innerWidth < 768) {
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
