import { Component, EventEmitter, HostListener, OnDestroy, OnInit, Output, computed, inject } from '@angular/core';
import { AuthService } from '../../../core/services/auth.service';
import { ClockService } from '../../../core/services/clock.service';

const AUTO_CLOSE_MS = 8000;
/** Set in sessionStorage after a successful login so the layout shows the popup once. */
export const WELCOME_FLAG_KEY = 'sip_welcome';

@Component({
  selector: 'app-welcome-dialog',
  standalone: true,
  templateUrl: './welcome-dialog.component.html',
  styleUrl: './welcome-dialog.component.scss',
})
export class WelcomeDialogComponent implements OnInit, OnDestroy {
  private readonly auth = inject(AuthService);
  readonly clock = inject(ClockService);

  @Output() closed = new EventEmitter<void>();

  readonly firstName = computed(() => this.auth.user()?.firstName ?? 'there');
  readonly roleLabel = computed(() => (this.auth.isAdmin() ? 'Administrator' : 'Staff'));
  protected readonly autoCloseSeconds = AUTO_CLOSE_MS / 1000;

  private timer?: ReturnType<typeof setTimeout>;

  ngOnInit(): void {
    this.timer = setTimeout(() => this.close(), AUTO_CLOSE_MS);
  }

  ngOnDestroy(): void {
    clearTimeout(this.timer);
  }

  @HostListener('document:keydown.escape')
  close(): void {
    clearTimeout(this.timer);
    this.closed.emit();
  }
}
