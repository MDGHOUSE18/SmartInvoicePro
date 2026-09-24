import { Injectable, inject } from '@angular/core';
import { ApiService } from './api.service';
import { AuditLog, AuditLogFilter } from '../models/audit-log.model';
import { PagedResult } from '../models/api-response.model';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class AuditLogService {
  private readonly api = inject(ApiService);

  getAll(filter: AuditLogFilter): Observable<PagedResult<AuditLog>> {
    return this.api.get<PagedResult<AuditLog>>('auditlogs', filter as Record<string, string | number>);
  }
}
