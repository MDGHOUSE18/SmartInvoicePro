import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';

export interface TableColumn {
  key: string;
  label: string;
  sortable?: boolean;
  align?: 'left' | 'right' | 'center';
  width?: string;
}

@Component({
  selector: 'app-data-table',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './data-table.component.html',
  styleUrl: './data-table.component.scss',
})
export class DataTableComponent {
  @Input({ required: true }) columns: TableColumn[] = [];
  @Input({ required: true }) data: Record<string, unknown>[] = [];
  @Input() loading = false;
  @Input() emptyMessage = 'No records found';
  @Input() page = 1;
  @Input() totalPages = 1;
  @Input() sortKey = '';
  @Input() sortDir: 'asc' | 'desc' = 'asc';
  @Input() trackByFn: (row: Record<string, unknown>) => string | number = (row) => JSON.stringify(row);

  @Output() sortChange = new EventEmitter<{ key: string; dir: 'asc' | 'desc' }>();
  @Output() pageChange = new EventEmitter<number>();

  trackBy(row: Record<string, unknown>): string | number {
    return this.trackByFn(row);
  }

  getCellValue(row: Record<string, unknown>, key: string): unknown {
    return row[key];
  }

  onSort(key: string): void {
    const dir = this.sortKey === key && this.sortDir === 'asc' ? 'desc' : 'asc';
    this.sortChange.emit({ key, dir });
  }
}
