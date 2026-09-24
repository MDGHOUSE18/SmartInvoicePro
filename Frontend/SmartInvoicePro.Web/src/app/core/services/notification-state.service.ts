import { Injectable, inject, signal } from '@angular/core';
import { ApiService } from './api.service';
import { AppNotification } from '../models/notification.model';
import { PagedResult } from '../models/api-response.model';

@Injectable({ providedIn: 'root' })
export class NotificationStateService {
  private readonly api = inject(ApiService);

  readonly unreadCount = signal(0);
  readonly recentNotifications = signal<AppNotification[]>([]);
  readonly loading = signal(false);

  refreshUnreadCount(): void {
    this.api.get<number>('notifications/unread-count').subscribe({
      next: (count) => this.unreadCount.set(count),
      error: () => this.unreadCount.set(0),
    });
  }

  loadRecent(limit = 5): void {
    this.loading.set(true);
    this.api.get<PagedResult<AppNotification>>('notifications', { page: 1, pageSize: limit }).subscribe({
      next: (result) => {
        this.recentNotifications.set(result.items);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  markRead(id: string): void {
    this.api.put<unknown>(`notifications/${id}/read`).subscribe({
      next: () => {
        this.recentNotifications.update((items) =>
          items.map((n) => (n.notificationId === id ? { ...n, isRead: true } : n))
        );
        this.refreshUnreadCount();
      },
    });
  }

  markAllRead(): void {
    this.api.put<unknown>('notifications/read-all').subscribe({
      next: () => {
        this.recentNotifications.update((items) => items.map((n) => ({ ...n, isRead: true })));
        this.unreadCount.set(0);
      },
    });
  }
}
