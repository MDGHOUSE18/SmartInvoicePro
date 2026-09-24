import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { getStatusClass } from '../../../core/utils/format.utils';

@Component({
  selector: 'app-status-badge',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './status-badge.component.html',
  styleUrl: './status-badge.component.scss',
})
export class StatusBadgeComponent {
  @Input({ required: true }) status = '';
  protected readonly getStatusClass = getStatusClass;
}
