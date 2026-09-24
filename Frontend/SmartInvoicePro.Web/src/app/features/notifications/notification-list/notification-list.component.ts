import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NotificationService } from '../../../core/services/notification.service';
import { NotificationStateService } from '../../../core/services/notification-state.service';
import { AppNotification } from '../../../core/models/notification.model';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { formatDate } from '../../../core/utils/format.utils';

@Component({
  selector: 'app-notification-list',
  standalone: true,
  imports: [CommonModule, PageHeaderComponent],
  templateUrl: './notification-list.component.html',
  styleUrl: './notification-list.component.scss',
})
export class NotificationListComponent implements OnInit {
  private readonly notificationService = inject(NotificationService);
  private readonly notifState = inject(NotificationStateService);

  readonly notifications = signal<AppNotification[]>([]);
  readonly loading = signal(false);
  protected readonly formatDate = formatDate;

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading.set(true);
    this.notificationService.getAll(1, 50).subscribe({
      next: (r) => { this.notifications.set(r.items); this.loading.set(false); },
      error: () => this.loading.set(false),
    });
  }

  markRead(n: AppNotification): void {
    if (n.isRead) return;
    this.notificationService.markRead(n.notificationId).subscribe({
      next: () => {
        this.notifications.update((items) => items.map((i) => (i.notificationId === n.notificationId ? { ...i, isRead: true } : i)));
        this.notifState.refreshUnreadCount();
      },
    });
  }

  markAllRead(): void {
    this.notificationService.markAllRead().subscribe({
      next: () => {
        this.notifications.update((items) => items.map((i) => ({ ...i, isRead: true })));
        this.notifState.unreadCount.set(0);
      },
    });
  }
}
