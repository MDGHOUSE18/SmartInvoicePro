import { Component } from '@angular/core';
import { AnimateOnScrollDirective } from '../../directives/animate-on-scroll.directive';

@Component({
  selector: 'app-landing-workflow',
  standalone: true,
  imports: [AnimateOnScrollDirective],
  templateUrl: './landing-workflow.component.html',
  styleUrl: './landing-workflow.component.scss',
})
export class LandingWorkflowComponent {
  readonly steps = [
    { title: 'Create Customer', description: 'Add client details and billing preferences', icon: '👤' },
    { title: 'Create Invoice', description: 'Build professional invoices with line items', icon: '📝' },
    { title: 'Send Invoice', description: 'Email directly or share a secure link', icon: '📤' },
    { title: 'Receive Payment', description: 'Track payments and send reminders', icon: '💵' },
    { title: 'Generate Reports', description: 'Analyze performance with visual reports', icon: '📊' },
  ];
}
