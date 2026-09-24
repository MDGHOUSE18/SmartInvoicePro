import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DemoService } from '../../../core/services/demo.service';
import { getApiErrorMessage } from '../../../core/utils/api-error';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'app-demo-reset',
  standalone: true,
  imports: [CommonModule, PageHeaderComponent, ConfirmDialogComponent],
  templateUrl: './demo-reset.component.html',
  styleUrl: './demo-reset.component.scss',
})
export class DemoResetComponent {
  private readonly demoService = inject(DemoService);
  readonly confirmOpen = signal(false);
  readonly message = signal('');
  readonly error = signal('');
  readonly loading = signal(false);

  reset(): void {
    this.loading.set(true);
    this.confirmOpen.set(false);
    this.demoService.reset().subscribe({
      next: (msg) => { this.loading.set(false); this.message.set(msg ?? 'Demo data reset successfully'); },
      error: (err) => { this.loading.set(false); this.error.set(getApiErrorMessage(err, 'Reset failed')); },
    });
  }
}
