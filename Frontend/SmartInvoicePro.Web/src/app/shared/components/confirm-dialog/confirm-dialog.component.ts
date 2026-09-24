import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-confirm-dialog',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './confirm-dialog.component.html',
  styleUrl: './confirm-dialog.component.scss',
})
export class ConfirmDialogComponent {
  @Input() open = false;
  @Input() title = 'Confirm';
  @Input() message = 'Are you sure?';
  @Input() confirmLabel = 'Confirm';
  @Input() cancelLabel = 'Cancel';
  @Input() danger = false;
  @Output() confirmed = new EventEmitter<void>();
  @Output() cancelled = new EventEmitter<void>();

  onConfirm(): void {
    this.confirmed.emit();
  }

  onCancel(): void {
    this.cancelled.emit();
  }
}
