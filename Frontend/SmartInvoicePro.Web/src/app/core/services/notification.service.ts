import { Injectable, inject } from '@angular/core';
import { ApiService } from './api.service';
import { AppNotification } from '../models/notification.model';
import { PagedResult } from '../models/api-response.model';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class NotificationService {
  private readonly api = inject(ApiService);

  getAll(page = 1, pageSize = 20, unreadOnly?: boolean): Observable<PagedResult<AppNotification>> {
    return this.api.get<PagedResult<AppNotification>>('notifications', { page, pageSize, unreadOnly });
  }

  markRead(id: string): Observable<void> {
    return this.api.put(`notifications/${id}/read`);
  }

  markAllRead(): Observable<void> {
    return this.api.put('notifications/read-all');
  }
}
