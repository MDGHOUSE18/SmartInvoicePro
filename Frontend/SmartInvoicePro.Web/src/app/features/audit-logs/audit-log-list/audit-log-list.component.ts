import { Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { AuditLogService } from '../../../core/services/audit-log.service';
import { AuditLog } from '../../../core/models/audit-log.model';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { formatDate } from '../../../core/utils/format.utils';

@Component({
  selector: 'app-audit-log-list',
  standalone: true,
  imports: [CommonModule, FormsModule, PageHeaderComponent],
  templateUrl: './audit-log-list.component.html',
  styleUrl: './audit-log-list.component.scss',
})
export class AuditLogListComponent implements OnInit {
  private readonly auditService = inject(AuditLogService);
  readonly logs = signal<AuditLog[]>([]);
  readonly loading = signal(false);
  readonly page = signal(1);
  readonly totalPages = signal(1);
  entityName = '';
  action = '';
  protected readonly formatDate = formatDate;

  ngOnInit(): void { this.load(1); }

  load(p: number): void {
    this.loading.set(true);
    this.page.set(p);
    this.auditService.getAll({ page: p, pageSize: 20, entityName: this.entityName || undefined, action: this.action || undefined }).subscribe({
      next: (r) => { this.logs.set(r.items); this.totalPages.set(r.totalPages); this.loading.set(false); },
      error: () => this.loading.set(false),
    });
  }
}
