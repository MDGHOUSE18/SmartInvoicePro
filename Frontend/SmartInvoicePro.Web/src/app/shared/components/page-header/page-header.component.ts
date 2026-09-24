import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-page-header',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './page-header.component.html',
  styleUrl: './page-header.component.scss',
})
export class PageHeaderComponent {
  @Input({ required: true }) title = '';
  @Input() subtitle = '';
  @Input() actionLabel = '';
  @Input() actionLink = '';
}
