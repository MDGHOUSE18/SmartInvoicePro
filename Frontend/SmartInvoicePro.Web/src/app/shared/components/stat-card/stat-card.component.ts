import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-stat-card',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './stat-card.component.html',
  styleUrl: './stat-card.component.scss',
})
export class StatCardComponent {
  @Input({ required: true }) label = '';
  @Input({ required: true }) value = '';
  @Input() subtitle = '';
  @Input() icon = '📊';
  @Input() iconBg = 'rgba(13, 148, 136, 0.15)';
}
