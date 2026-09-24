import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../../core/services/auth.service';
import { getApiErrorMessage } from '../../../core/utils/api-error';

@Component({
  selector: 'app-reset-password',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './reset-password.component.html',
  styleUrl: './reset-password.component.scss',
})
export class ResetPasswordComponent {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  readonly loading = signal(false);
  readonly error = signal('');
  readonly message = signal('');
  private readonly token = this.route.snapshot.queryParamMap.get('token') ?? '';

  readonly form = this.fb.nonNullable.group({
    newPassword: ['', [Validators.required, Validators.minLength(8)]],
  });

  submit(): void {
    if (this.form.invalid || !this.token) {
      this.error.set('Invalid or missing reset token');
      return;
    }
    this.loading.set(true);
    this.auth.resetPassword({ token: this.token, newPassword: this.form.value.newPassword! }).subscribe({
      next: (msg) => {
        this.loading.set(false);
        this.message.set(msg ?? 'Password reset successful');
        setTimeout(() => this.router.navigate(['/auth/login']), 2000);
      },
      error: (err) => {
        this.loading.set(false);
        this.error.set(getApiErrorMessage(err, 'Reset failed'));
      },
    });
  }
}
