export interface AuditLog {
  auditLogId: number;
  entityName: string;
  entityId: string;
  action: string;
  oldValues?: string;
  newValues?: string;
  userId?: string;
  userEmail?: string;
  ipAddress?: string;
  createdDate: string;
}

export interface AuditLogFilter {
  entityName?: string;
  action?: string;
  userId?: string;
  fromDate?: string;
  toDate?: string;
  page?: number;
  pageSize?: number;
}
