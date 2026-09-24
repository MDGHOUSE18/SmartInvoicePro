import { Component, EventEmitter, Output, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../core/services/auth.service';
import { ThemeService } from '../../core/services/theme.service';
import { NotificationStateService } from '../../core/services/notification-state.service';
import { ClockService } from '../../core/services/clock.service';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './header.component.html',
  styleUrl: './header.component.scss',
})
export class HeaderComponent implements OnInit {
  readonly auth = inject(AuthService);
  readonly theme = inject(ThemeService);
  readonly notifState = inject(NotificationStateService);
  readonly clock = inject(ClockService);
  readonly showNotif = signal(false);

  @Output() menuToggle = new EventEmitter<void>();

  ngOnInit(): void {
    this.notifState.refreshUnreadCount();
    this.notifState.loadRecent();
  }

  toggleNotif(): void {
    this.showNotif.update((v) => !v);
  }

  markRead(id: string): void {
    this.notifState.markRead(id);
  }
}
